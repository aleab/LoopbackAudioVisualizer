using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Maths;
using CSCore.DSP;
using MathNet.Numerics;
using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01
{
    public class SpectrumVisualizer : BaseSpectrumVisualizer
    {
        private const FftSize REFERENCE_FFTSIZE = FftSize.Fft1024;

        #region Inspector

        [SerializeField]
        private ScaleUpObject cubePrefab;

        [SerializeField]
        private Vector3 center = Vector3.zero;

        [SerializeField]
        [Range(10.0f, 200.0f)]
        private float radius = 10.0f;

        [SerializeField]
        [Range(0.0f, 500.0f)]
        private float maxYScale = 90.0f;

        #region Equalization

        [SerializeField]
        [Range(0.1f, 25.0f)]
        private float lowFreqGain = 9.0f;

        [SerializeField]
        [Range(0.1f, 25.0f)]
        private float highFreqGain = 18.0f;

        [SerializeField]
        private FunctionType equalizationFunctionType = FunctionType.Gaussian;

        [SerializeField]
        [Range(1.0f, 50.0f)]
        private float gaussStdDeviation = 35.0f;

        [SerializeField]
        [Range(1.0f, 20.0f)]
        private float logSteepness = 10.0f;

        #endregion Equalization

        #endregion Inspector

        private Coroutine updateCubesCoroutine;
        private ScaleUpObject[] cubes;

        private void Awake()
        {
            this.RequireField(nameof(this.cubePrefab), this.cubePrefab);
        }

        protected override void Start()
        {
            GameObject cubes = SpawnRadialCubes((int)this.fftSize / 2, this.center, this.radius, this.cubePrefab.gameObject, this.gameObject.transform);
            this.cubes = new ScaleUpObject[cubes.transform.childCount];
            for (int i = 0; i < this.cubes.Length; ++i)
                this.cubes[i] = cubes.transform.GetChild(i).gameObject.GetComponent<ScaleUpObject>();
        }

        protected float EqualizationFunction(int fftBandIndex, float fftBandValue)
        {
            float f = this.spectrumProvider.GetFrequency(fftBandIndex);

            float gain = 1.0f;
            const float k = 1000.0f; // scale

            switch (this.equalizationFunctionType)
            {
                case FunctionType.Gaussian:
                    float gaussLowPeak = k * (this.lowFreqGain - this.highFreqGain);
                    double gaussVariance = Math.Pow(this.gaussStdDeviation, 2);
                    gain = (float)(gaussLowPeak * Math.Exp(-Math.Pow(f / k, 2) / (2.0 * gaussVariance)) + k * this.highFreqGain);
                    break;

                case FunctionType.Logarithm:
                    const float logBase = 10.0f;
                    const double freqAtMaxGain = 48000.0;
                    float logScale = k * this.logSteepness;
                    double lowFreqPow = Math.Pow(10, this.lowFreqGain * k / logScale);
                    double highFreqPow = Math.Pow(10, this.highFreqGain * k / logScale);
                    gain = (float)(logScale * Math.Log((highFreqPow - lowFreqPow) * (f / freqAtMaxGain) + lowFreqPow, logBase));
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

            GameObject cubes = new GameObject($"{n}Cubes");
            cubes.transform.parent = parent;
            cubes.transform.position = center;
            for (int i = 0; i < n; ++i)
            {
                cubes.transform.localEulerAngles = new Vector3(0.0f, -deltaDegrees * i, 0.0f);

                GameObject cube = Instantiate(cubePrefab);
                cube.name = $"Cube{i}";
                cube.transform.localScale = new Vector3(squareSide, squareSide, squareSide);
                cube.transform.position = cubes.transform.position + Vector3.forward * radius;
                cube.transform.parent = cubes.transform;
                cube.SetActive(true);
            }

            return cubes;
        }
    }
}