using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Visualizers
{
    /// <summary>
    /// Base class for amplitude visualizers.
    /// The <code>filteredSamples</code> variable contains the latest audio samples captured by <see cref="LoopbackAudioSource"/> and filtered with the <see cref="Filter"/> function.
    /// </summary>
    public class BaseAmplitudeVisualizer : MonoBehaviour
    {
        public const float FILTER_BASE_MIN = 2.0f;
        public const float FILTER_BASE_MAX = 10.0f;

        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private bool useFilter = true;

        [SerializeField]
        [Range(0.0f, 10.0f)]
        private float sensitivity = 1.0f;

        [SerializeField]
        [Range(FILTER_BASE_MIN, FILTER_BASE_MAX)]
        private float filterBase = (float)Math.E;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float filterK = 0.75f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float filterMean = 0.7f;

        [SerializeField]
        [Range(0.1f, 2.0f)]
        private float filterStdDeviation = 0.95f;

        [SerializeField]
        [Range(0.0f, 5.0f)]
        private float filterLambda = 1.5f;

#pragma warning disable 0649

        #endregion Inspector

        private Coroutine updateAudioSamplesCoroutine;

        protected LoopbackAudioSource loopbackAudioSource;

        protected AudioBlock filteredSamples = AudioBlock.Zero;

        public int Channels { get { return this.filteredSamples.samples?.Length ?? -1; } }

        protected virtual void Awake()
        {
            this.loopbackAudioSource = FindObjectOfType<LoopbackAudioSource>();
            if (this.loopbackAudioSource == null)
            {
                Debug.LogError($"[{nameof(BaseAmplitudeVisualizer)}] Couldn't find LoopbackAudioSource.");
                Destroy(this.gameObject);
            }
        }

        protected virtual void Start()
        {
            this.updateAudioSamplesCoroutine = this.StartCoroutine(this.UpdateAudioSamples());
        }

        private IEnumerator UpdateAudioSamples()
        {
            Func<float, float> ClampFilter = x => Mathf.Clamp01(this.useFilter ? this.Filter(x * this.sensitivity, false) : x * this.sensitivity);

            yield return null;
            while (this.updateAudioSamplesCoroutine != null)
            {
                var currentAudioBlock = this.loopbackAudioSource.CurrentAudioBlock.Abs();
                this.filteredSamples.left = this.filteredSamples.samples[0] = ClampFilter(currentAudioBlock.left);
                this.filteredSamples.right = this.filteredSamples.samples[1] = ClampFilter(currentAudioBlock.right);
                for (int i = 2; i < this.filteredSamples.samples.Length; ++i)
                    this.filteredSamples.samples[i] = ClampFilter(this.filteredSamples.samples[i]);

                yield return new WaitForSeconds(0.05f);
            }
        }

        protected virtual void Destroy()
        {
            this.updateAudioSamplesCoroutine = null;
        }

        protected float GaussianFilter(float x, float mean, float standardDeviation) => 1.0f / (Mathf.Sqrt(2 * Mathf.PI) * standardDeviation) * Mathf.Pow(this.filterBase, -Mathf.Pow(x - mean, 2) / (2 * standardDeviation * standardDeviation));

        protected float ExponentialFilter(float x, float lambda) => x >= 0 ? lambda * Mathf.Pow(this.filterBase, -lambda * x) : 0.0f;

        /// <summary>
        /// Filters a value combining the Gaussian filter and the Exponential filter.
        /// </summary>
        /// <param name="x"> The independent variable. </param>
        /// <param name="filterZero"> Whether to filter the value if it's zero or not. </param>
        /// <returns> The filtered value. </returns>
        protected virtual float Filter(float x, bool filterZero = true) => x + (x > 0 || filterZero ? (x <= this.filterK ? this.GaussianFilter(x, this.filterMean, this.filterStdDeviation) : this.ExponentialFilter(x, this.filterLambda)) : 0.0f);
    }
}