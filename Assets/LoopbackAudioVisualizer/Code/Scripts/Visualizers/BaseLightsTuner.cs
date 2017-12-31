using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.Visualizers
{
    public abstract class BaseLightsTuner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private bool autoUpdate;

        [SerializeField]
        protected LightSet[] lightSets;

        #endregion Inspector

        private Coroutine lightsUpdateCoroutine;

        public bool AutoUpdate { get { return this.autoUpdate; } }

        protected virtual float UpdateInterval { get { return 0.05f; } }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (this.AutoUpdate && this.lightsUpdateCoroutine == null)
                this.lightsUpdateCoroutine = this.StartCoroutine(this.LightsUpdateCoroutine());
#endif
        }

        protected virtual void OnEnable()
        {
            if (this.AutoUpdate && this.lightsUpdateCoroutine == null)
                this.lightsUpdateCoroutine = this.StartCoroutine(this.LightsUpdateCoroutine());
        }

        protected virtual void OnDisable()
        {
            if (this.lightsUpdateCoroutine != null)
            {
                this.StopCoroutine(this.lightsUpdateCoroutine);
                this.lightsUpdateCoroutine = null;
            }
        }

        protected abstract void TuneLight(int setIndex, Light light);

        protected void UpdateLights()
        {
            if (this.lightSets != null)
            {
                for (int i = 0; i < this.lightSets.Length; ++i)
                    this.UpdateLights(i);
            }
        }

        protected void UpdateLights(int setIndex)
        {
            if (this.lightSets?[setIndex]?.lights != null)
            {
                foreach (var light in this.lightSets[setIndex].lights)
                    this.TuneLight(setIndex, light);
            }
        }

        private IEnumerator LightsUpdateCoroutine()
        {
            yield return null;
            while (this.AutoUpdate)
            {
                this.UpdateLights();
                yield return new WaitForSeconds(this.UpdateInterval);
            }
            this.lightsUpdateCoroutine = null;
        }

        [Serializable]
        protected class LightSet
        {
            public Light[] lights;
        }
    }
}