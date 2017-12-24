using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Maths;
using CSCore.DSP;
using MathNet.Numerics;
using System;
using System.Collections;
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
    public class SpectrumVisualizer : BaseSpectrumVisualizer
    {
        private const FftSize REFERENCE_FFTSIZE = FftSize.Fft1024;

        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        [DisableWhenPlaying]
        private ScaleUpObject cubePrefab;

        [SerializeField]
        [DisableWhenPlaying]
        private Transform center;

        [SerializeField]
        [DisableWhenPlaying]
        [Range(1.0f, 100.0f)]
        private float radius = 10.0f;

        [SerializeField]
        [Range(0.0f, 500.0f)]
        private float maxYScale = 125.0f;

        #region Equalization

        [SerializeField]
        private FunctionType equalizationFunctionType = FunctionType.Gaussian;

        #region Gaussian

        [SerializeField]
        [Range(1.0f, 50.0f)]
        private float gaussStdDeviation = 9.0f;

        [SerializeField]
        [Range(0.1f, 15.0f)]
        private float gaussLowFreqGain = 9.0f;

        [SerializeField]
        [Range(10.0f, 50.0f)]
        private float gaussHighFreqGain = 32.0f;

        #endregion Gaussian

        #region Logarithmic

        [SerializeField]
        [Range(1.0f, 20.0f)]
        private float logSteepness = 10.0f;

        [SerializeField]
        [Range(2.0f, 48.0f)]
        private float logHighFreq = 44.1f;

        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float logLowFreqGain = 9.0f;

        [SerializeField]
        [Range(0.1f, 25.0f)]
        private float logHighFreqGain = 18.0f;

        #endregion Logarithmic

        #endregion Equalization

#pragma warning restore 0414, 0649

        #endregion Inspector

        private Coroutine updateCubesCoroutine;
        private ScaleUpObject[] cubes;

        protected virtual void Awake()
        {
            this.RequireField(nameof(this.cubePrefab), this.cubePrefab);
            this.RequireField(nameof(this.center), this.center);
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            base.Start();

            GameObject cubes = SpawnRadialCubes((int)this.fftSize / 2, Vector3.zero, this.radius, this.cubePrefab.gameObject, this.gameObject.transform);
            this.cubes = new ScaleUpObject[cubes.transform.childCount];
            for (int i = 0; i < this.cubes.Length; ++i)
                this.cubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<ScaleUpObject>();
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                this.UpdateEditorCubesParentContainer();
#endif
        }

        protected float EqualizationFunction(int fftBandIndex, float fftBandValue)
        {
            float f = this.spectrumProvider.GetFrequency(fftBandIndex);

            float gain = 1.0f;
            const float k = 100.0f; // scale

            switch (this.equalizationFunctionType)
            {
                case FunctionType.Gaussian:
                    float gaussLowPeak = k * (this.gaussLowFreqGain - this.gaussHighFreqGain);
                    double gaussVariance = Math.Pow(this.gaussStdDeviation, 2);
                    gain = (float)(gaussLowPeak * Math.Exp(-Math.Pow(f / 1000.0, 2) / (2.0 * gaussVariance)) + k * this.gaussHighFreqGain);
                    break;

                case FunctionType.Logarithm:
                    const double logBase = 10.0;
                    float logScale = k * this.logSteepness;
                    double lowFreqPow = Math.Pow(logBase, (this.logLowFreqGain * k) / logScale);
                    double highFreqPow = Math.Pow(logBase, (this.logHighFreqGain * k) / logScale);
                    gain = (float)(logScale * Math.Log((highFreqPow - lowFreqPow) * (f / (this.logHighFreq * 1000.0)) + lowFreqPow, logBase));
                    break;
            }
            return fftBandValue * gain;
        }

        private IEnumerator UpdateCubes()
        {
            yield return null;
            float fftSizeRatio = (float)this.fftSize / (float)REFERENCE_FFTSIZE;
            while (this.updateCubesCoroutine != null)
            {
                for (int i = 0; i < this.cubes.Length && i < this.fftDataBuffer.Length; ++i)
                {
                    float scaledFftValue = this.EqualizationFunction(i, this.fftDataBuffer[i]) * fftSizeRatio;
                    this.cubes[i].ScaleSmooth(this.maxYScale < 0.0f ? scaledFftValue : Math.Min(scaledFftValue, this.maxYScale), true);
                }
                yield return new WaitForSeconds(UPDATE_FFT_INTERVAL);
            }
        }

        protected void ResetCubes()
        {
            if (this.cubes != null)
            {
                foreach (var cube in this.cubes)
                    cube.Scale(cube.MinimumScale);
            }
        }

        protected override void LoopbackAudioSource_DeviceChanged(object sender, MMDeviceChangedEventArgs e)
        {
            if (this.updateCubesCoroutine != null)
            {
                this.StopCoroutine(this.updateCubesCoroutine);
                this.updateCubesCoroutine = null;
            }

            base.LoopbackAudioSource_DeviceChanged(sender, e);

            if (e.Initialized)
                this.updateCubesCoroutine = this.StartCoroutine(this.UpdateCubes());
            else
                this.ResetCubes();
        }

        /// <summary>
        /// Spaws cubes upon a circumference.
        /// </summary>
        /// <param name="n"> Number of cubes. </param>
        /// <param name="center"> Center of the circumference in local coordinates. </param>
        /// <param name="radius"> Radius of the circumference. </param>
        /// <param name="cubePrefab"> Cubes template. </param>
        /// <param name="parent"> Parent of the cubes structure. </param>
        /// <returns> Returns the GameObject containing the cubes, whose parent is 'parent'. </returns>
        protected static GameObject SpawnRadialCubes(int n, Vector3 center, float radius, GameObject cubePrefab, Transform parent = null)
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

            // Create the cubes container, child of parent
            GameObject cubes = new GameObject($"{n}Cubes");
            cubes.transform.parent = parent;
            cubes.transform.localPosition = center;

            // Create the cubes
            for (int i = 0; i < n; ++i)
            {
                cubes.transform.localEulerAngles = new Vector3(0.0f, -deltaDegrees * i, 0.0f);

                GameObject cube = Instantiate(cubePrefab);
                cube.name = $"Cube{i}";
                cube.SetActive(true); // Awake it now, to let it store the prefab's original scale
                cube.transform.localScale = new Vector3(squareSide, squareSide, squareSide);
                cube.transform.position = cubes.transform.position + Vector3.forward * radius;
                cube.transform.parent = cubes.transform;
            }
            cubes.transform.localRotation = Quaternion.identity;

            // Re-set parent's original local rotation
            if (parent != null)
                parent.localRotation = parentRotation;

            return cubes;
        }

        #region ExecuteInEditMode

#if UNITY_EDITOR

        private const string EDITOR_CUBES_PARENT_CONTAINER_NAME = "EditorOnly_Cubes";

        [NonSerialized]
        private GameObject editorCubesParentContainer;

        [NonSerialized]
        private ScaleUpObject[] editorCubes;

        private void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                this.StartCoroutine(this.CreateEditorCubes());
        }

        private IEnumerator CreateEditorCubes()
        {
            yield return null;

            // Load EditorOnly scene and find editorCubesParentContainer
            if (!Scenes.AudioVisualizer01_EditorOnly.IsLoaded())
                this.LoadEditorOnlyScene();
            this.FindOrCreateEditorCubesParentContainer();

            // Fake temporary spectrum provider
            this.spectrumProvider = new SimpleSpectrumProvider(2, 48000, this.fftSize);

            // Create the cubes' parent inside the container in the EditorOnly scene
            this.editorCubes = null;
            Transform cubesParentTransform = this.editorCubesParentContainer.transform.Find("ExampleCubes");
            GameObject cubesParent = cubesParentTransform?.gameObject;
            if (cubesParent != null)
                DestroyImmediate(cubesParent);

            if (this.cubePrefab != null)
            {
                cubesParent = SpawnRadialCubes((int)this.fftSize / 2, Vector3.zero, this.radius, this.cubePrefab.gameObject, this.editorCubesParentContainer.transform);
                cubesParent.name = "ExampleCubes";
                cubesParent.AddComponent<UnityInspectorOnly>();
                this.editorCubes = new ScaleUpObject[cubesParent.transform.childCount];

                // Randomly create a plausible spectrum for preview.
                for (int i = 0; i < this.editorCubes.Length; ++i)
                {
                    this.editorCubes[i] = cubesParent.transform.GetChild(i).gameObject.GetComponent<ScaleUpObject>();

                    float currFreq = this.spectrumProvider.GetFrequency(i);
                    float rndValue = (float)MathNet.Numerics.Random.MersenneTwister.Default.NextDouble();
                    float g1 = (float)(+0.009 * Math.Exp(-Math.Pow(currFreq / 1000 - 2.25, 2) / (2 * 4.60 * 4.60)));
                    float g2 = (float)(+0.000 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.00, 2) / (2 * 0.35 * 0.35)));
                    float g3 = (float)(+0.005 * Math.Exp(-Math.Pow(currFreq / 1000 - 0.85, 2) / (2 * 0.85 * 0.85)));
                    float g4 = (float)(-0.008 * Math.Exp(-Math.Pow(currFreq / 1000 - 3.50, 2) / (2 * 5.00 * 5.00)));
                    float gaussMult = Math.Abs(g1 + g2 + g3 + g4);
                    float rndScaledValue = this.EqualizationFunction(i, rndValue * gaussMult);
                    this.editorCubes[i].Scale((this.maxYScale < 0.0f ? rndScaledValue : Math.Min(rndScaledValue, this.maxYScale)) * this.editorCubes[i].gameObject.transform.localScale.y);
                }

                this.UpdateEditorCubesParentContainer();
            }

            this.spectrumProvider = null;
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
                this.FindOrCreateEditorCubesParentContainer();
                SceneManager.SetActiveScene(currentScene);
                EditorSceneManager.SaveScene(editorOnlyScene, Scenes.AudioVisualizer01_EditorOnly.Path);
            }
        }

        private void FindOrCreateEditorCubesParentContainer()
        {
            this.editorCubesParentContainer = GameObject.Find(EDITOR_CUBES_PARENT_CONTAINER_NAME);
            if (this.editorCubesParentContainer == null)
            {
                this.editorCubesParentContainer = new GameObject(EDITOR_CUBES_PARENT_CONTAINER_NAME)
                {
                    isStatic = true,
                    transform = { parent = null }
                };
                this.editorCubesParentContainer.AddComponent<UnityInspectorOnly>();
            }

            this.editorCubesParentContainer.transform.localPosition = Vector3.zero;
            this.editorCubesParentContainer.transform.localRotation = Quaternion.identity;
            this.editorCubesParentContainer.transform.localScale = Vector3.one;
        }

        private void UpdateEditorCubesParentContainer()
        {
            if (this.editorCubesParentContainer != null)
            {
                this.editorCubesParentContainer.transform.position = this.gameObject.transform.position;
                this.editorCubesParentContainer.transform.localRotation = this.gameObject.transform.localRotation;
                this.editorCubesParentContainer.transform.localScale = this.gameObject.transform.localScale;
            }
        }

#endif

        #endregion ExecuteInEditMode
    }
}