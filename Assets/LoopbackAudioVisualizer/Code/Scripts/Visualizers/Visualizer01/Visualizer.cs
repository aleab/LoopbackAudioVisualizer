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

        [SerializeField]
        [DisableWhenPlaying]
        private EmissiveScaleUpObject cubePrefab;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform cubesContainer;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform center;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(1.0f, 100.0f)]
        private float radius = 11.0f;

        [SerializeField]
        [Range(0.0f, 500.0f)]
        private float maxYScale = 11.0f;

#pragma warning restore 0414, 0649

        #endregion Inspector

        private Material cubesMaterial;

        private EmissiveScaleUpObject[] cubes;

        private void Awake()
        {
            this.RequireField(nameof(this.spectrumVisualizer), this.spectrumVisualizer);
            this.RequireField(nameof(this.cubePrefab), this.cubePrefab);
            this.RequireField(nameof(this.center), this.center);

            if (this.cubesContainer == null)
                this.cubesContainer = this.gameObject.transform;

#if UNITY_EDITOR
            // If ExecuteInEditMode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            // Apply a copy of the shared material to the cube prefab
            this.cubesMaterial = new Material(this.cubePrefab.MeshRenderer.sharedMaterial);
            this.cubesMaterial.name += " (Instance)";
            this.cubePrefab.MeshRenderer.sharedMaterial = this.cubesMaterial;

            this.spectrumVisualizer.FftBandScaled += this.ScaledSpectrumVisualizer_FftBandScaled;
            this.spectrumVisualizer.FftDataBufferUpdated += this.SpectrumVisualizer_FftDataBufferUpdated;
            this.spectrumVisualizer.UpdateFftDataCoroutineStarted += this.ScaledSpectrumVisualizer_UpdateFftDataCoroutineStarted;
            this.spectrumVisualizer.UpdateFftDataCoroutineStopped += this.ScaledSpectrumVisualizer_UpdateFftDataCoroutineStopped;
        }

        private void Start()
        {
#if UNITY_EDITOR
            // If ExecuteInEditMode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                this.CreateEditorCubes();
                return;
            }
#endif

            GameObject cubes = SpawnRadialCubes((int)this.spectrumVisualizer.FftSize / 2, this.center.position, this.radius, this.cubePrefab.gameObject, this.cubesContainer);
            this.cubes = new EmissiveScaleUpObject[cubes.transform.childCount];
            for (int i = 0; i < this.cubes.Length; ++i)
                this.cubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();
            this.ResetCubes();
        }

        private float LightsIntensityFunction(float value)
        {
            value = 1.8f * Mathf.Log10(2.6f * value + 1);
            return float.IsNaN(value) || float.IsNegativeInfinity(value) ? 0.0f :
                   float.IsPositiveInfinity(value) ? 1.0f : value;
        }

        private float CubesEmissionValueFunction(float value)
        {
            const double k = 0.75;
            const float minValue = 0.40f;
            const float maxValue = 0.84f;
            value = (float)(k * Math.Log10((Math.Pow(10.0, 1 / k) - 1.0) * value + 1.0));
            return minValue + (maxValue - minValue) * value;
        }

        private void ResetCubes()
        {
            if (this.cubes != null)
            {
                foreach (var cube in this.cubes)
                {
                    cube.Scale(cube.MinimumScale);
                    cube.SetLightsIntensity(this.LightsIntensityFunction(0.0f));
                }
            }
        }

        private void RenameCubes(IReadOnlyList<ScaleUpObject> cubes = null)
        {
#if UNITY_EDITOR
            if (cubes == null)
                cubes = this.cubes;

            if (cubes != null)
            {
                SimpleSpectrumProvider spectrumProvider = this.spectrumVisualizer.SpectrumProvider;
                for (int i = 0; i < cubes.Count; ++i)
                    cubes[i].gameObject.name = $"Cube{i,-4} @ {spectrumProvider.GetFrequency(i),+5:N0}Hz";
            }
#endif
        }

        private void UpdateCubesEmissionValue()
        {
            if (this.cubes != null && this.cubes.Length > 0)
            {
                float maxSpectrumValue = this.spectrumVisualizer.ScaledFftDataBuffer.Max();
                float normalizedSpectrumAverageAmplitude = this.spectrumVisualizer.SpectrumMeanAmplitude / (this.maxYScale <= 0.0f ? maxSpectrumValue : this.maxYScale);

                if (this.cubePrefab.UseSharedMaterial)
                    this.cubes[0].SetEmissionColor(HSVChannel.Value, this.CubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                else
                {
                    foreach (var cube in this.cubes)
                        cube.SetEmissionColor(HSVChannel.Value, this.CubesEmissionValueFunction(normalizedSpectrumAverageAmplitude));
                }
            }
        }

        #region Event Handlers

        private void ScaledSpectrumVisualizer_UpdateFftDataCoroutineStarted(object sender, EventArgs e)
        {
            this.RenameCubes();
        }

        private void ScaledSpectrumVisualizer_UpdateFftDataCoroutineStopped(object sender, EventArgs e)
        {
            this.ResetCubes();
        }

        private void ScaledSpectrumVisualizer_FftBandScaled(object sender, FftBandScaledEventArgs e)
        {
            float clampedScaledValue = Math.Min(e.ScaledValue, this.maxYScale);
            float maxSpectrumValue = this.spectrumVisualizer.ScaledFftDataBuffer.Max();

            this.cubes[e.Index].ScaleSmooth(this.maxYScale < 0.0f ? e.ScaledValue : clampedScaledValue, true);
            this.cubes[e.Index].SetLightsIntensity(this.LightsIntensityFunction(this.maxYScale < 0.0f ? e.ScaledValue / maxSpectrumValue : clampedScaledValue / this.maxYScale));
        }

        private void SpectrumVisualizer_FftDataBufferUpdated(object sender, EventArgs e)
        {
            this.UpdateCubesEmissionValue();
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
        private static GameObject SpawnRadialCubes(int n, Vector3 center, float radius, GameObject cubePrefab, Transform parent = null)
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
            cubesContainer.name = $"{n}Cubes";
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

        #region ExecuteInEditMode

#if UNITY_EDITOR

        private const string EDITOR_CUBES_CONTAINER_NAME = "EditorOnly_Cubes";

        [NonSerialized]
        private GameObject editorCubesContainer;

        [NonSerialized]
        private EmissiveScaleUpObject[] editorCubes;

        public void CreateEditorCubes()
        {
            if (this.editorCubesContainer != null)
            {
                DestroyImmediate(this.editorCubesContainer);
                this.editorCubesContainer = null;
            }

            // Load EditorOnly scene and find editorCubesParentContainer
            if (!Scenes.AudioVisualizer01_EditorOnly.IsLoaded())
                this.LoadEditorOnlyScene();
            if (this.editorCubesContainer == null)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(Scenes.AudioVisualizer01_EditorOnly.Name));
                this.CreateEditorCubesContainer();
                SceneManager.SetActiveScene(currentScene);
            }

            // Create the cubes' parent inside the container in the EditorOnly scene
            this.editorCubes = null;

            if (this.cubePrefab != null)
            {
                GameObject cubes = SpawnRadialCubes((int)this.spectrumVisualizer.FftSize / 2, Vector3.zero, this.radius, this.cubePrefab.gameObject, this.editorCubesContainer.transform);
                this.editorCubesContainer.name = EDITOR_CUBES_CONTAINER_NAME;
                this.editorCubes = new EmissiveScaleUpObject[cubes.transform.childCount];

                SimpleSpectrumProvider spectrumProvider = this.spectrumVisualizer.SpectrumProvider;

                // Randomly create a plausible spectrum for preview.
                for (int i = 0; i < this.editorCubes.Length; ++i)
                {
                    this.editorCubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<EmissiveScaleUpObject>();

                    float currFreq = spectrumProvider.GetFrequency(i);
                    float rndValue = (float)MathNet.Numerics.Random.MersenneTwister.Default.NextDouble();
                    float g1 = (float)(+0.0085 * Math.Exp(-Math.Pow(currFreq / 1000 - 2.25, 2) / (2 * 4.60 * 4.60)));
                    float g2 = (float)(-0.0005 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.00, 2) / (2 * 0.38 * 0.38)));
                    float g3 = (float)(+0.0046 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.85, 2) / (2 * 0.85 * 0.85)));
                    float g4 = (float)(-0.0070 * Math.Exp(-Math.Pow(currFreq / 1000 - 3.50, 2) / (2 * 5.00 * 5.00)));
                    float gaussMult = Math.Abs(g1 + g2 + g3 + g4);
                    float rndScaledValue = this.spectrumVisualizer.SpectrumScalingFunction(i, rndValue * gaussMult);
                    float clampedScaledValue = Math.Min(rndScaledValue, this.maxYScale);
                    this.editorCubes[i].Scale(this.maxYScale < 0.0f ? rndScaledValue : Math.Min(rndScaledValue, this.maxYScale));
                    this.editorCubes[i].SetLightsIntensity(this.LightsIntensityFunction(this.maxYScale < 0.0f ? rndScaledValue / this.spectrumVisualizer.ScaledFftDataBuffer.Max() : clampedScaledValue / this.maxYScale));
                }
                this.RenameCubes(this.editorCubes);

                this.UpdateEditorCubesParentContainer();
            }
        }

        private void LoadEditorOnlyScene()
        {
            Scene editorOnlyScene = Scenes.AudioVisualizer01_EditorOnly.Load(LoadSceneMode.Additive);
            if (!editorOnlyScene.IsValid())
            {
                Scene currentScene = SceneManager.GetActiveScene();

                // Create and save the scene
                editorOnlyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(editorOnlyScene);
                this.CreateEditorCubesContainer();
                SceneManager.SetActiveScene(currentScene);
                EditorSceneManager.SaveScene(editorOnlyScene, Scenes.AudioVisualizer01_EditorOnly.Path);
            }
        }

        private void CreateEditorCubesContainer()
        {
            this.editorCubesContainer = GameObject.Find(EDITOR_CUBES_CONTAINER_NAME);
            if (this.editorCubesContainer != null)
                DestroyImmediate(this.editorCubesContainer);

            this.editorCubesContainer = new GameObject(EDITOR_CUBES_CONTAINER_NAME)
            {
                isStatic = false,
                transform = { parent = null }
            };
            this.editorCubesContainer.AddComponent<UnityInspectorOnly>();

            this.editorCubesContainer.transform.localPosition = Vector3.zero;
            this.editorCubesContainer.transform.localRotation = Quaternion.identity;
            this.editorCubesContainer.transform.localScale = Vector3.one;
        }

        private void UpdateEditorCubesParentContainer()
        {
            if (this.editorCubesContainer != null)
            {
                this.editorCubesContainer.transform.position = (this.cubesContainer ?? this.gameObject.transform).position;
                this.editorCubesContainer.transform.localRotation = (this.cubesContainer ?? this.gameObject.transform).localRotation;
                this.editorCubesContainer.transform.localScale = (this.cubesContainer ?? this.gameObject.transform).localScale;
            }
        }

#endif

        #endregion ExecuteInEditMode
    }
}