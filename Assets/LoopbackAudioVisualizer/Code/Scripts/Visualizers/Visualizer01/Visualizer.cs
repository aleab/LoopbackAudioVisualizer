using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using Aleab.LoopbackAudioVisualizer.Common;
using Aleab.LoopbackAudioVisualizer.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Range = Aleab.LoopbackAudioVisualizer.Unity.RangeAttribute;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

#endif

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    [ExecuteInEditMode]
    public sealed class Visualizer : MonoBehaviour
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        [DisableWhenPlaying]
        private SpectrumVisualizer spectrumVisualizer;

        #region Circumference Spectrum

        [SerializeField]
        [DisableWhenPlaying]
        private EmissiveScaleUpObject circCubeTemplate;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform circCubesContainer;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform circCenter;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(1.0f, 20.0f)]
        private float circRadius = 10.0f;

        [SerializeField]
        [Range(0.0f, 100.0f)]
        private float circCubeMaxHeight = 12.0f;

        #endregion Circumference Spectrum

        #region Ellipse Arc Spectrum

        [SerializeField]
        [DisableWhenPlaying]
        private EmissiveScaleUpObject ellipseCubeTemplate;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform ellipseCubesContainer;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform ellipseCenter;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(1.0f, 20.0f)]
        private float ellipseSemiMinorAxisLength = 8.0f;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(1.0f, 20.0f)]
        private float ellipseChordLength = 12.0f;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(80, 160)]
        private int ellipseArcDegree = 120;

        [SerializeField]
        [Range(0.0f, 100.0f)]
        private float ellipseCubeMaxHeight = 8.0f;

        #endregion Ellipse Arc Spectrum

#pragma warning restore 0414, 0649

        #endregion Inspector

        private void Awake()
        {
            this.RequireField(nameof(this.spectrumVisualizer), this.spectrumVisualizer);
            this.RequireField(nameof(this.circCubeTemplate), this.circCubeTemplate);
            this.RequireField(nameof(this.circCenter), this.circCenter);

            if (this.circCubesContainer == null)
                this.circCubesContainer = this.gameObject.transform;

#if UNITY_EDITOR
            // If ExecuteInEditMode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            // Apply a copy of the shared material to the circumference cube prefab
            this.circCubesMaterial = new Material(this.circCubeTemplate.MeshRenderer.sharedMaterial);
            this.circCubesMaterial.name += " (Instance)";
            this.circCubeTemplate.MeshRenderer.sharedMaterial = this.circCubesMaterial;

            // Apply a copy of the shared material to the ellipse cube prefab
            this.ellipseCubesMaterial = new Material(this.ellipseCubeTemplate.MeshRenderer.sharedMaterial);
            this.ellipseCubesMaterial.name += " (Instance)";
            this.ellipseCubeTemplate.MeshRenderer.sharedMaterial = this.ellipseCubesMaterial;

            // Events
            this.spectrumVisualizer.UpdateFftDataCoroutineStarted += this.ScaledSpectrumVisualizer_UpdateFftDataCoroutineStarted;
            this.spectrumVisualizer.UpdateFftDataCoroutineStopped += this.ScaledSpectrumVisualizer_UpdateFftDataCoroutineStopped;
            this.spectrumVisualizer.FftBandScaled += this.ScaledSpectrumVisualizer_FftBandScaled;
            this.spectrumVisualizer.FftDataBufferUpdated += this.SpectrumVisualizer_FftDataBufferUpdated;
            this.spectrumVisualizer.SpectrumMeanAmplitudeUpdated += this.SpectrumVisualizer_SpectrumMeanAmplitudeUpdated;
            this.spectrumVisualizer.BandValueCalculated += this.SpectrumVisualizer_BandValueCalculated;
        }

        private void Start()
        {
#if UNITY_EDITOR
            // If ExecuteInEditMode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                this.CreateEditorCircCubes();
                this.CreateEditorEllipseCubes();
                return;
            }
#endif

            GameObject circCubesParent = SpawnCircCubes((int)this.spectrumVisualizer.FftSize / 2, this.circCenter.position, this.circRadius, this.circCubeTemplate.gameObject, this.circCubesContainer);
            this.circCubes = new EmissiveScaleUpObject[circCubesParent.transform.childCount];
            for (int i = 0; i < this.circCubes.Length; ++i)
                this.circCubes[i] = circCubesParent.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();
            this.ResetCircCubes();

            GameObject ellipseCubesParent = SpawnEllipseCubes(this.spectrumVisualizer.NumberOfBands, this.ellipseCenter.position, this.ellipseChordLength, this.ellipseSemiMinorAxisLength, this.ellipseArcDegree, this.ellipseCubeTemplate.gameObject, this.ellipseCubesContainer);
            this.ellipseCubes = new EmissiveScaleUpObject[ellipseCubesParent.transform.childCount];
            for (int i = 0; i < this.ellipseCubes.Length; ++i)
                this.ellipseCubes[i] = ellipseCubesParent.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();
            this.ResetEllipseCubes();
        }

        #region { Circumference Spectrum }

        private Material circCubesMaterial;

        private EmissiveScaleUpObject[] circCubes;

        private float CircCubesLightsIntensityFunction(float value)
        {
            value = 1.8f * Mathf.Log10(2.6f * value + 1);
            return float.IsNaN(value) || float.IsNegativeInfinity(value) ? 0.0f :
                   float.IsPositiveInfinity(value) ? 1.0f : value;
        }

        private float CircCubesEmissionValueFunction(float value)
        {
            const double k = 0.75;
            const float minValue = 0.40f;
            const float maxValue = 0.84f;
            value = (float)(k * Math.Log10((Math.Pow(10.0, 1 / k) - 1.0) * value + 1.0));
            return minValue + (maxValue - minValue) * value;
        }

        private void ResetCircCubes()
        {
            if (this.circCubes != null)
            {
                foreach (var cube in this.circCubes)
                {
                    cube.Scale(cube.MinimumScale);
                    cube.SetLightsIntensity(this.CircCubesLightsIntensityFunction(0.0f));
                }
            }
        }

        private void RenameCircCubes(IReadOnlyList<ScaleUpObject> cubes = null)
        {
#if UNITY_EDITOR
            if (cubes == null)
                cubes = this.circCubes;

            if (cubes != null)
            {
                SimpleSpectrumProvider spectrumProvider = this.spectrumVisualizer.SpectrumProvider;
                for (int i = 0; i < cubes.Count; ++i)
                    cubes[i].gameObject.name = $"Cube{i,-4} @ {spectrumProvider.GetFrequency(i),+5:N0}Hz";
            }
#endif
        }

        private void UpdateCircCubesEmissionValue()
        {
            if (this.circCubes != null && this.circCubes.Length > 0)
            {
                float maxSpectrumValue = this.spectrumVisualizer.ScaledFftDataBuffer.Max();
                float normalizedSpectrumAverageAmplitude = this.spectrumVisualizer.SpectrumMeanAmplitude / (this.circCubeMaxHeight <= 0.0f ? maxSpectrumValue : this.circCubeMaxHeight);

                if (this.circCubeTemplate.UseSharedMaterial)
                    this.circCubes[0].SetEmissionColor(HSVChannel.Value, this.CircCubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                else
                {
                    foreach (var cube in this.circCubes)
                        cube.SetEmissionColor(HSVChannel.Value, this.CircCubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                }
            }
        }

        #endregion { Circumference Spectrum }

        #region { Ellipse Arc Spectrum }

        private Material ellipseCubesMaterial;

        private EmissiveScaleUpObject[] ellipseCubes;

        private float EllipseCubesLightsIntensityFunction(float value)
        {
            value = 1.8f * Mathf.Log10(2.6f * value + 1);
            return float.IsNaN(value) || float.IsNegativeInfinity(value) ? 0.0f :
                float.IsPositiveInfinity(value) ? 1.0f : value;
        }

        private float EllipseCubesEmissionValueFunction(float value)
        {
            const double k = 0.75;
            const float minValue = 0.255f;
            const float maxValue = 0.915f;
            value = (float)(k * Math.Log10((Math.Pow(10.0, 1 / k) - 1.0) * value + 1.0));
            return minValue + (maxValue - minValue) * value;
        }

        private void ResetEllipseCubes()
        {
            if (this.ellipseCubes != null)
            {
                foreach (var cube in this.ellipseCubes)
                {
                    cube.Scale(cube.MinimumScale);
                    cube.SetLightsIntensity(this.EllipseCubesLightsIntensityFunction(0.0f));
                }
            }
        }

        private void UpdateEllipseCubesEmissionValue()
        {
            if (this.ellipseCubes != null && this.ellipseCubes.Length > 0)
            {
                float maxSpectrumValue = this.spectrumVisualizer.BandsDataBuffer.Max();
                float normalizedSpectrumAverageAmplitude = this.spectrumVisualizer.SpectrumMeanAmplitude / (this.ellipseCubeMaxHeight <= 0.0f ? maxSpectrumValue : this.ellipseCubeMaxHeight);

                if (this.ellipseCubeTemplate.UseSharedMaterial)
                    this.ellipseCubes[0].SetEmissionColor(HSVChannel.Value, this.EllipseCubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                else
                {
                    foreach (var cube in this.ellipseCubes)
                        cube.SetEmissionColor(HSVChannel.Value, this.EllipseCubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                }
            }
        }

        #endregion { Ellipse Arc Spectrum }

        #region Event Handlers

        private void ScaledSpectrumVisualizer_UpdateFftDataCoroutineStarted(object sender, EventArgs e)
        {
            this.RenameCircCubes();
        }

        private void ScaledSpectrumVisualizer_UpdateFftDataCoroutineStopped(object sender, EventArgs e)
        {
            this.ResetCircCubes();
            this.ResetEllipseCubes();
        }

        private void ScaledSpectrumVisualizer_FftBandScaled(object sender, FftBandScaledEventArgs e)
        {
            float clampedScaledValue = Math.Min(e.ScaledValue, this.circCubeMaxHeight);
            float maxSpectrumValue = this.spectrumVisualizer.ScaledFftDataBuffer.Max();

            this.circCubes[e.Index].ScaleSmooth(this.circCubeMaxHeight <= 0.0f ? e.ScaledValue : clampedScaledValue, true);
            this.circCubes[e.Index].SetLightsIntensity(this.CircCubesLightsIntensityFunction(this.circCubeMaxHeight <= 0.0f ? e.ScaledValue / maxSpectrumValue : clampedScaledValue / this.circCubeMaxHeight));
        }

        private void SpectrumVisualizer_FftDataBufferUpdated(object sender, EventArgs e)
        {
        }

        private void SpectrumVisualizer_SpectrumMeanAmplitudeUpdated(object sender, EventArgs e)
        {
            this.UpdateCircCubesEmissionValue();
            this.UpdateEllipseCubesEmissionValue();
        }

        private void SpectrumVisualizer_BandValueCalculated(object sender, BandValueCalculatedEventArgs e)
        {
            /*
             * g(x) = Clamp[f(x), 0, maxCirc]
             * clampedScaledValue(x) = g(x) / k
             *    k such that when g(x) is at its maximum (maxCirc), clampedScaledValue(maxCirc) equals to maxEllipse
             *    k = maxCirc / maxEllipse
             *
             * => clampedScaledValue(x) = (maxEllipse / maxCirc) Clamp[f(x), 0, maxCirc]
             */

            float maxBandsValue = this.spectrumVisualizer.BandsDataBuffer.Max();
            float maxSpectrumValue = this.spectrumVisualizer.ScaledFftDataBuffer.Max();
            float clampedBandValue = this.ellipseCubeMaxHeight / (this.circCubeMaxHeight <= 0.0f ? maxSpectrumValue : this.circCubeMaxHeight) * Math.Min(e.Value, this.circCubeMaxHeight);

            this.ellipseCubes[e.BandIndex].ScaleSmooth(this.ellipseCubeMaxHeight <= 0.0f ? e.Value : clampedBandValue, true);
            this.ellipseCubes[e.BandIndex].SetLightsIntensity(this.EllipseCubesLightsIntensityFunction(this.ellipseCubeMaxHeight <= 0.0f ? e.Value / maxBandsValue : clampedBandValue / this.ellipseCubeMaxHeight));
        }

        #endregion Event Handlers

        /// <summary>
        /// Spaws cubes upon a circumference.
        /// </summary>
        /// <param name="n"> Number of cubes. </param>
        /// <param name="center"> Center of the circumference in local coordinates. </param>
        /// <param name="radius"> Radius of the circumference. </param>
        /// <param name="cubePrefab"> Cubes template. </param>
        /// <param name="parent"> Parent of the cubes structure. </param>
        /// <returns> Returns the GameObject containing the cubes, whose parent is 'parent'. </returns>
        private static GameObject SpawnCircCubes(int n, Vector3 center, float radius, GameObject cubePrefab, Transform parent = null)
        {
            #region Maths

            /*
             * C: center; R: radius; δ = 360°/n: deltaDegree
             * P, Q: first and second points on the circumference
             * t: bisectrix of angle PCQ
             * M: parametric point on t
             * p: line through M, perpendicular to PC
             * J: intersection of p and segment PC
             * d: distance from point M to point J
             * l: distance from point P to point J
             *
             * CONDITIONS:
             *  1) 0° ≤ δ ≤ 90°
             *     There should be at least 4 cubes
             *  2) d == l
             *     MJ must be half of the square's side
             *  3) length of segment MC <= R
             *     M must be inside the circumference
             *
             *
             * C = [0, 0];  P = [0, R];  Q = [-R sin(δ), R cos(δ)]
             * random point on bisectrix: B = [-R sin(δ/2), R cos(δ/2)]
             * t: y = -(R cos(δ/2) / R sin(δ/2)) x = -x cotg(δ/2)
             * M = k * B = [-k R sin(δ/2), k R cos(δ/2)]
             * p: y = k R cos(δ/2)
             * J = [0, k R cos(δ/2)]
             *
             * 1)
             * 0° ≤ δ/2 ≤ 45° => 1 ≥ cos(δ/2) ≥ √2/2
             *                   0 ≤ sin(δ/2) ≤ √2/2
             *
             * 2)     ____________________________________________________
             * { d = √(0 + k R sin(δ/2))² + (k R cos(δ/2) - k R cos(δ/2))² = | k R sin(δ/2) |
             * {      ______________________________
             * { l = √(0 - 0)² + (k R cos(δ/2) - R)² = | k R cos(δ/2) - R|
             *
             * | k R sin(δ/2) | = | k R cos(δ/2) - R |
             * k R cos(δ/2) - R = ± k R sin(δ/2)
             * k (cos(δ/2) ∓ sin(δ/2)) = 1
             * k = 1 / (cos(δ/2) ∓ sin(δ/2))
             *
             * 1+2)
             * k > 0 always ✓
             *
             * 3)
             * ‖M‖ = ‖k * B‖ < R
             *  _______________________________
             * √k²R² sin²(δ/2) + k²R² cos²(δ/2) < R
             * k R < R
             * k < 1
             *
             * 2+3)
             * k < 1  <=>  cos(δ/2) ∓ sin(δ/2) > 1
             * hence, must be  k = 1 / (cos(δ/2) + sin(δ/2))
             *
             *
             * CONCLUSION:
             *   square side = 2 * length of MJ
             *   M = [-(cos(δ/2) + sin(δ/2))⁻¹ R sin(δ/2), (cos(δ/2) + sin(δ/2))⁻¹ R cos(δ/2)]
             *   J = [0, (cos(δ/2) + sin(δ/2))⁻¹ R cos(δ/2)]
             *                   _____________________________________________
             *   length of MJ = √(0 + (cos(δ/2) + sin(δ/2))⁻¹ R sin(δ/2))² + 0 = (cos(δ/2) + sin(δ/2))⁻¹ R sin(δ/2)
             *   square side = 2 R sin(δ/2) (cos(δ/2) + sin(δ/2))⁻¹
             *
             */

            #endregion Maths

            float deltaDegrees = 360.0f / n;
            double deltaRadians = Trig.DegreeToRadian(deltaDegrees);
            float squareSide = (float)(2.0 * radius * Math.Sin(deltaRadians / 2) * (1 / (Math.Cos(deltaRadians / 2) + Math.Sin(deltaRadians / 2))));

            // Save parent's current local rotation; it will be re-set later
            Quaternion parentRotation = parent?.localRotation ?? Quaternion.identity;
            if (parent != null)
                parent.localRotation = Quaternion.identity;

            GameObject cubesContainer = parent?.gameObject ?? new GameObject();
            cubesContainer.name = $"Circumference_{n}Cubes";
            cubesContainer.transform.localPosition = center;

            // Create the cubes
            for (int i = 0; i < n; ++i)
            {
                cubesContainer.transform.localEulerAngles = new Vector3(0.0f, -deltaDegrees * i, 0.0f);

                GameObject cube = Instantiate(cubePrefab);
                cube.name = $"Cube{i}";
                cube.SetActive(true); // Awake it now, to let it store the prefab's original scale
                cube.transform.localScale = new Vector3(squareSide, squareSide, squareSide);
                cube.transform.position = cubesContainer.transform.position + Vector3.forward * radius;
                cube.transform.parent = cubesContainer.transform;
            }
            cubesContainer.transform.localRotation = Quaternion.identity;

            // Re-set parent's original local rotation
            if (parent != null)
                parent.localRotation = parentRotation;

            return cubesContainer;
        }

        /// <summary>
        /// Spawns cubes upon an arc of an ellipse.
        /// </summary>
        /// <param name="n"> The number of cubes. </param>
        /// <param name="center"> The center of the ellipse. </param>
        /// <param name="sectorChordLength"> The length of the arc's chord. </param>
        /// <param name="semiMinorAxisLength"> Half the length of the minor axis. </param>
        /// <param name="sectorAngleDeg"> The angular length of the arc in degrees. </param>
        /// <param name="cubePrefab"> The template for the cubes. </param>
        /// <param name="parent"> The parent of the cubes structure. </param>
        /// <returns> Returns the GameObject containing the cubes, whose parent is 'parent'. </returns>
        /// <remarks> This function is definitely very time-expensive. Use sparingly. </remarks>
        private static GameObject SpawnEllipseCubes(int n, Vector3 center, float sectorChordLength, float semiMinorAxisLength, float sectorAngleDeg, GameObject cubePrefab, Transform parent = null)
        {
            // TODO: Find constraints for every parameter and variable (minimum and maximum values)
            //       Problem: for some combination of input parameters, the FindRoots.OfFunction method fails with an exception:
            //                "ArithmeticException: Function does not accept floating point Not-a-Number values."
            //       Sample parameters: (16,, 12.0f, 6.0f, 90°,,)

            #region Maths

            /*
             * C: center of the ellipse
             * R: sectorChordLength, the length of the chord
             * a: half the length of the major axis of the ellipse
             * b: semiMinorAxisLength, half the length of the minor axis of the ellipse
             * α: sectorAngleDeg, the sector extension in degrees, from (90° - α/2) to (90° + α/2) counterclock-wise
             * n: number of cubes
             *
             *
             * § 1) ELLIPSE'S EQUATIONS
             *
             * ξ(ϑ): ellipse's parametric equations [-180° ≤ ϑ ≤ 180°]
             *     where ϑ is the angle of (ξˣ(ϑ), ξʸ(ϑ)) with the major axis of the ellipse itself
             *                       ___________________________
             *   ξˣ(ϑ) = a b cos(ϑ) √(a² sin²(ϑ) + b² cos²(ϑ))⁻¹
             *                       ___________________________
             *   ξʸ(ϑ) = a b sin(ϑ) √(a² sin²(ϑ) + b² cos²(ϑ))⁻¹
             *
             * A = ξ(90° + α/2)
             * B = ξ(90° - α/2)
             *
             *     __                   _______________________________
             * R = AB = 2 a b sin(α/2) √(a² cos²(α/2) + b² sin²(α/2))⁻¹
             *      _________________________________________                   _______________________
             * a = √(b² R² tan²(α/2)) / (4 b² tan²(α/2) - R²) = (b R tan(α/2)) √(4 b² tan²(α/2) - R²)⁻¹
             *
             *
             * § 2) CUBES PLACEMENT
             *      A _____________________
             * L = ∫(√D[ξˣ(ϑ)]² + D[ξʸ(ϑ)]²)dϑ: length of arc AB
             *    B
             * l = L/n: distance between each cube's center on the arc
             *
             * θᵢ: angle of the iᵗʰ arc segment; arc length to the iᵗʰ arc segment = i l
             *                  _____________________
             * θᵢ = FindRoot[∫(√D[ξˣ(ϑ)]² + D[ξʸ(ϑ)]²)dϑ = i l]   😭
             *
             * Pᵢ = ξ(θᵢ): position of the iᵗʰ cube
             *
             *
             * § 3) CUBES' SIDE LENGTH
             * Let Pᵢ and Pⱼ be the positions of two consecutive cubes (j = i + 1) with angles θᵢ and θⱼ.
             * Let Pₖ be a third point placed at the same arc-distance from both Pᵢ and Pⱼ; the angle of Pₖ is θₖ.
             *
             * rₖ: line from C to Pₖ
             * rᵢ: line from C to Pᵢ
             * rⱼ: line from C to Pⱼ
             *
             * M: parametric point on rₖ
             * dᵢ: segment MPᵢ
             * dⱼ: segment MPⱼ
             *
             *
             * M = (mˣ, mʸ) = (mˣ, mˣ tan(θₖ))
             *       ____________________________________
             * dᵢ = √(x(Pᵢ) - mˣ)² + (y(Pᵢ) - mˣ tan(θₖ))²
             *       ____________________________________
             * dⱼ = √(x(Pⱼ) - mˣ)² + (y(Pⱼ) - mˣ tan(θₖ))²
             *
             * ASSERTION
             * dᵢ = dⱼ = d: dᵢ and dⱼ are the semi-diagonals of the two squares
             *
             * (x(Pᵢ) - mˣ)² + (y(Pᵢ) - mˣ tan(θₖ))² = (x(Pⱼ) - mˣ)² + (y(Pⱼ) - mˣ tan(θₖ))²
             * x(Pᵢ)² + (mˣ)² - 2 x(Pᵢ) mˣ + y(Pᵢ)² + (mˣ tan(θₖ))² - 2 y(Pᵢ) mˣ tan(θₖ)  =  x(Pⱼ)² + (mˣ)² - 2 x(Pⱼ) mˣ + y(Pⱼ)² + (mˣ tan(θₖ))² - 2 y(Pⱼ) mˣ tan(θₖ)
             * x(Pᵢ)² - 2 x(Pᵢ) mˣ + y(Pᵢ)² - 2 y(Pᵢ) mˣ tan(θₖ)  =  x(Pⱼ)² - 2 x(Pⱼ) mˣ + y(Pⱼ)² - 2 y(Pⱼ) mˣ tan(θₖ)
             * 2 x(Pⱼ) mˣ + 2 y(Pⱼ) mˣ tan(θₖ) - 2 x(Pᵢ) mˣ - 2 y(Pᵢ) mˣ tan(θₖ)  =  x(Pⱼ)² + y(Pⱼ)² - x(Pᵢ)² - y(Pᵢ)²
             * 2 mˣ (x(Pⱼ) + y(Pⱼ) tan(θₖ) - x(Pᵢ) - y(Pᵢ) tan(θₖ))  =  x(Pⱼ)² + y(Pⱼ)² - x(Pᵢ)² - y(Pᵢ)²
             *
             *              x(Pⱼ)² + y(Pⱼ)² - x(Pᵢ)² - y(Pᵢ)²
             * mˣ = ——————————————————————————————————————————————————
             *       2 (x(Pⱼ) + y(Pⱼ) tan(θₖ) - x(Pᵢ) - y(Pᵢ) tan(θₖ))
             *
             * cube's side length = 2 (d/√2)
             *
             */

            #endregion Maths

            double R = sectorChordLength;
            double b = semiMinorAxisLength;

            #region § 1) ELLIPSE'S EQUATION

            double alphaRad = Trig.DegreeToRadian(sectorAngleDeg);
            double bTanHalfAlpha = b * Trig.Tan(alphaRad / 2.0);
            double a = (R * bTanHalfAlpha) / Math.Sqrt(4 * bTanHalfAlpha * bTanHalfAlpha - R * R);

            Func<double, double> EllipseX = t =>
            {
                double sin = Trig.Sin(t);
                double cos = Trig.Cos(t);
                double aSin = a * sin;
                double bCos = b * cos;
                return (a * bCos) / Math.Sqrt(aSin * aSin + bCos * bCos);
            };
            Func<double, double> EllipseY = t =>
            {
                double sin = Trig.Sin(t);
                double cos = Trig.Cos(t);
                double aSin = a * sin;
                double bCos = b * cos;
                return (b * aSin) / Math.Sqrt(aSin * aSin + bCos * bCos);
            };

            #endregion § 1) ELLIPSE'S EQUATION

            #region § 2) CUBES PLACEMENT

            Func<double, double> DEllipseX = Differentiate.FirstDerivativeFunc(EllipseX);
            Func<double, double> DEllipseY = Differentiate.FirstDerivativeFunc(EllipseY);

            Func<double, double> SqrtOfSumOfSquaredDerivatives = t =>
            {
                double x = DEllipseX(t);
                double y = DEllipseY(t);
                return Math.Sqrt(x * x + y * y);
            };

            Func<double, double, double> AngleToArcLength = (fromRad, toRad) => Integrate.OnClosedInterval(SqrtOfSumOfSquaredDerivatives, fromRad, toRad);
            Func<double, double> ArcLengthToAngle = arcLength => FindRoots.OfFunction(toRad => AngleToArcLength((Math.PI - alphaRad) / 2, toRad) - arcLength, 0, Math.PI);

            double L = AngleToArcLength((Math.PI - alphaRad) / 2, (Math.PI + alphaRad) / 2);
            double l = L / (n - 1);

            Func<int, object[]> CalculateCubePositionAndAngle = i =>
            {
                double angle = ArcLengthToAngle(i * l);
                Vector2 position = new Vector2((float)EllipseX(angle), (float)EllipseY(angle));
                return new object[] { position, angle };
            };

            #endregion § 2) CUBES PLACEMENT

            #region § 3) CUBES' SIDE LENGTH

            Vector2[] positions = new Vector2[n];
            double[] angles = new double[n];
            double[] middleAngles = new double[n - 1];
            for (int i = 0; i < n; ++i)
            {
                object[] res = CalculateCubePositionAndAngle(i);
                positions[i] = (Vector2)res[0];
                angles[i] = (double)res[1];

                if (i > 0)
                    middleAngles[i - 1] = ArcLengthToAngle((2 * i - 1) * l / 2.0);
            }

            double[] sideLengths = new double[n - 1];
            Func<int, double> SideLength = j =>
            {
                double xj = positions[j].x;
                double yj = positions[j].y;
                double xi = positions[j - 1].x;
                double yi = positions[j - 1].y;
                double tanK = Trig.Tan(middleAngles[j - 1]);
                double mx = (xj * xj + yj * yj - (xi * xi + yi * yi)) / (2.0 * (xj + yj * tanK - (xi + yi * tanK)));
                return Math.Sqrt((xi - mx) * (xi - mx) + (yi - mx * tanK) * (yi - mx * tanK)) * (2.0 / Math.Sqrt(2.0));
            };
            for (int i = 1; i < n; ++i)
                sideLengths[i - 1] = SideLength(i);

            float squareSide = (float)sideLengths.Min();

            #endregion § 3) CUBES' SIDE LENGTH

            // Save parent's current local rotation and scale; it will be re-set later
            Quaternion parentRotation = parent?.localRotation ?? Quaternion.identity;
            Vector3 parentScale = parent?.localScale ?? Vector3.one;
            if (parent != null)
            {
                parent.localRotation = Quaternion.identity;
                parent.localScale = Vector3.one;
            }

            GameObject cubesContainer = parent?.gameObject ?? new GameObject();
            cubesContainer.name = $"Ellipse_{n}Cubes";
            cubesContainer.transform.localPosition = center;

            // Create the cubes
            for (int i = 0; i < n; ++i)
            {
                GameObject cube = Instantiate(cubePrefab);
                cube.name = $"Cube{i}";
                cube.SetActive(true); // Awake it now, to let it store the prefab's original scale
                cube.transform.localScale = new Vector3(squareSide, squareSide, squareSide);
                cube.transform.position = cubesContainer.transform.position + new Vector3(positions[i].x, 0.0f, positions[i].y);
                cube.transform.localRotation = Quaternion.Euler(0.0f, (float)(90.0 - Trig.RadianToDegree(angles[i])), 0.0f);
                cube.transform.parent = cubesContainer.transform;
            }
            cubesContainer.transform.localRotation = Quaternion.identity;

            // Re-set parent's original local rotation
            if (parent != null)
            {
                parent.localRotation = parentRotation;
                parent.localScale = parentScale;
            }

            return cubesContainer;
        }

        #region ExecuteInEditMode

#if UNITY_EDITOR

        #region Circumference Cubes

        private const string EDITOR_CIRC_CUBES_CONTAINER_NAME = "EditorOnly_CircCubes";

        [NonSerialized]
        private GameObject editorCircCubesContainer;

        [NonSerialized]
        private EmissiveScaleUpObject[] editorCircCubes;

        public void CreateEditorCircCubes()
        {
            if (this.editorCircCubesContainer != null)
            {
                DestroyImmediate(this.editorCircCubesContainer);
                this.editorCircCubesContainer = null;
            }

            // Load EditorOnly scene and find editorCubesParentContainer
            if (!Scenes.AudioVisualizer01_EditorOnly.IsLoaded())
                this.LoadEditorOnlyScene();
            if (this.editorCircCubesContainer == null)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(Scenes.AudioVisualizer01_EditorOnly.Name));
                this.CreateEditorCircCubesContainer();
                SceneManager.SetActiveScene(currentScene);
            }

            // Create the cubes' parent inside the container in the EditorOnly scene
            this.editorCircCubes = null;

            if (this.circCubeTemplate != null)
            {
                GameObject cubes = SpawnCircCubes((int)this.spectrumVisualizer.FftSize / 2, Vector3.zero, this.circRadius, this.circCubeTemplate.gameObject, this.editorCircCubesContainer.transform);
                this.editorCircCubesContainer.name = EDITOR_CIRC_CUBES_CONTAINER_NAME;
                this.editorCircCubes = new EmissiveScaleUpObject[cubes.transform.childCount];

                SimpleSpectrumProvider spectrumProvider = this.spectrumVisualizer.SpectrumProvider;

                // Randomly create a plausible spectrum for preview.
                for (int i = 0; i < this.editorCircCubes.Length; ++i)
                {
                    this.editorCircCubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();

                    float currFreq = spectrumProvider.GetFrequency(i);
                    float rndValue = (float)MathNet.Numerics.Random.MersenneTwister.Default.NextDouble();
                    float g1 = (float)(+0.0085 * Math.Exp(-Math.Pow(currFreq / 1000 - 2.25, 2) / (2 * 4.60 * 4.60)));
                    float g2 = (float)(-0.0005 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.00, 2) / (2 * 0.38 * 0.38)));
                    float g3 = (float)(+0.0046 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.85, 2) / (2 * 0.85 * 0.85)));
                    float g4 = (float)(-0.0070 * Math.Exp(-Math.Pow(currFreq / 1000 - 3.50, 2) / (2 * 5.00 * 5.00)));
                    float gaussMult = Math.Abs(g1 + g2 + g3 + g4);
                    float rndScaledValue = this.spectrumVisualizer.SpectrumScalingFunction(i, rndValue * gaussMult);
                    float clampedScaledValue = Math.Min(rndScaledValue, this.circCubeMaxHeight);
                    this.editorCircCubes[i].Scale(this.circCubeMaxHeight <= 0.0f ? rndScaledValue : Math.Min(rndScaledValue, this.circCubeMaxHeight));
                    this.editorCircCubes[i].SetLightsIntensity(this.CircCubesLightsIntensityFunction(this.circCubeMaxHeight <= 0.0f ? rndScaledValue / this.spectrumVisualizer.ScaledFftDataBuffer.Max() : clampedScaledValue / this.circCubeMaxHeight));
                }
                this.RenameCircCubes(this.editorCircCubes);

                this.UpdateEditorCircCubesParentContainer();
            }
        }

        private void CreateEditorCircCubesContainer()
        {
            this.editorCircCubesContainer = GameObject.Find(EDITOR_CIRC_CUBES_CONTAINER_NAME);
            if (this.editorCircCubesContainer != null)
                DestroyImmediate(this.editorCircCubesContainer);

            this.editorCircCubesContainer = new GameObject(EDITOR_CIRC_CUBES_CONTAINER_NAME)
            {
                isStatic = false,
                transform = { parent = null }
            };
            this.editorCircCubesContainer.AddComponent<UnityInspectorOnly>();

            this.editorCircCubesContainer.transform.localPosition = Vector3.zero;
            this.editorCircCubesContainer.transform.localRotation = Quaternion.identity;
            this.editorCircCubesContainer.transform.localScale = Vector3.one;
        }

        private void UpdateEditorCircCubesParentContainer()
        {
            if (this.editorCircCubesContainer != null)
            {
                this.editorCircCubesContainer.transform.position = (this.circCubesContainer ?? this.gameObject.transform).position;
                this.editorCircCubesContainer.transform.localRotation = (this.circCubesContainer ?? this.gameObject.transform).localRotation;
                this.editorCircCubesContainer.transform.localScale = (this.circCubesContainer ?? this.gameObject.transform).localScale;
            }
        }

        #endregion Circumference Cubes

        #region Ellipse Cubes

        private const string EDITOR_ELLIPSE_CUBES_CONTAINER_NAME = "EditorOnly_EllipseCubes";

        [NonSerialized]
        private GameObject editorEllipseCubesContainer;

        [NonSerialized]
        private EmissiveScaleUpObject[] editorEllipseCubes;

        public void CreateEditorEllipseCubes()
        {
            if (this.editorEllipseCubesContainer != null)
            {
                DestroyImmediate(this.editorEllipseCubesContainer);
                this.editorEllipseCubesContainer = null;
            }

            // Load EditorOnly scene and find editorCubesParentContainer
            if (!Scenes.AudioVisualizer01_EditorOnly.IsLoaded())
                this.LoadEditorOnlyScene();
            if (this.editorEllipseCubesContainer == null)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(Scenes.AudioVisualizer01_EditorOnly.Name));
                this.CreateEditorEllipseCubesContainer();
                SceneManager.SetActiveScene(currentScene);
            }

            // Create the cubes' parent inside the container in the EditorOnly scene
            this.editorEllipseCubes = null;

            if (this.ellipseCubeTemplate != null)
            {
                GameObject cubes = SpawnEllipseCubes(this.spectrumVisualizer.NumberOfBands, Vector3.zero, this.ellipseChordLength, this.ellipseSemiMinorAxisLength, this.ellipseArcDegree, this.ellipseCubeTemplate.gameObject, this.editorEllipseCubesContainer.transform);
                this.editorEllipseCubesContainer.name = EDITOR_ELLIPSE_CUBES_CONTAINER_NAME;
                this.editorEllipseCubes = new EmissiveScaleUpObject[cubes.transform.childCount];

                // Randomly create a plausible spectrum for preview.
                double inc = 1.0 / this.editorEllipseCubes.Length;
                for (int i = 0; i < this.editorEllipseCubes.Length; ++i)
                {
                    this.editorEllipseCubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();

                    float rndValue = (float)MathNet.Numerics.Random.MersenneTwister.Default.NextDouble();
                    float g = (float)Math.Exp(-Math.Pow(i * inc - 0.25, 2) / (2 * 0.35 * 0.35));
                    float value = rndValue * g * this.ellipseCubeMaxHeight;
                    float clampedScaledValue = Math.Min(value, this.ellipseCubeMaxHeight);
                    this.editorEllipseCubes[i].Scale(this.ellipseCubeMaxHeight <= 0.0f ? value : Math.Min(value, this.ellipseCubeMaxHeight));
                    this.editorEllipseCubes[i].SetLightsIntensity(this.CircCubesLightsIntensityFunction(this.ellipseCubeMaxHeight <= 0.0f ? value / this.spectrumVisualizer.BandsDataBuffer.Max() : clampedScaledValue / this.ellipseCubeMaxHeight));
                }

                this.UpdateEditorEllipseCubesParentContainer();
            }
        }

        private void CreateEditorEllipseCubesContainer()
        {
            this.editorEllipseCubesContainer = GameObject.Find(EDITOR_ELLIPSE_CUBES_CONTAINER_NAME);
            if (this.editorEllipseCubesContainer != null)
                DestroyImmediate(this.editorEllipseCubesContainer);

            this.editorEllipseCubesContainer = new GameObject(EDITOR_ELLIPSE_CUBES_CONTAINER_NAME)
            {
                isStatic = false,
                transform = { parent = null }
            };
            this.editorEllipseCubesContainer.AddComponent<UnityInspectorOnly>();

            this.editorEllipseCubesContainer.transform.localPosition = Vector3.zero;
            this.editorEllipseCubesContainer.transform.localRotation = Quaternion.identity;
            this.editorEllipseCubesContainer.transform.localScale = Vector3.one;
        }

        private void UpdateEditorEllipseCubesParentContainer()
        {
            if (this.editorEllipseCubesContainer != null)
            {
                this.editorEllipseCubesContainer.transform.position = (this.ellipseCubesContainer ?? this.gameObject.transform).position;
                this.editorEllipseCubesContainer.transform.localRotation = (this.ellipseCubesContainer ?? this.gameObject.transform).localRotation;
                this.editorEllipseCubesContainer.transform.localScale = (this.ellipseCubesContainer ?? this.gameObject.transform).localScale;
            }
        }

        #endregion Ellipse Cubes

        private void LoadEditorOnlyScene()
        {
            Scene editorOnlyScene = Scenes.AudioVisualizer01_EditorOnly.Load(LoadSceneMode.Additive);
            if (!editorOnlyScene.IsValid())
            {
                Scene currentScene = SceneManager.GetActiveScene();

                // Create and save the scene
                editorOnlyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(editorOnlyScene);
                this.CreateEditorCircCubesContainer();
                this.CreateEditorEllipseCubesContainer();
                SceneManager.SetActiveScene(currentScene);
                EditorSceneManager.SaveScene(editorOnlyScene, Scenes.AudioVisualizer01_EditorOnly.Path);
            }
        }

#endif

        #endregion ExecuteInEditMode
    }
}