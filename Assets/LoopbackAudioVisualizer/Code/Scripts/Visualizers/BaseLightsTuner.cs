using Aleab.LoopbackAudioVisualizer.LightTuning;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        [SerializeField]
        private List<LightSetMapping> lightSetMappings = new List<LightSetMapping>();

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

        #region UpdateLights

        protected void UpdateLights()
        {
            if (this.lightSets != null)
            {
                foreach (var lightSet in this.lightSets)
                    lightSet?.ApplyTunings();
            }
        }

        protected void UpdateLights(int setIndex)
        {
            if (setIndex < 0 || setIndex >= (this.lightSets?.Length ?? -1))
                return;
            this.lightSets?[setIndex]?.ApplyTunings();
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

        #endregion UpdateLights

        #region LightSetMapping

        protected int GetLightSetIndex(string name)
        {
            return this.lightSetMappings.SingleOrDefault(m => m.Name == name)?.Index ?? -1;
        }

#if UNITY_EDITOR

        protected abstract string[] GetLightSetMappingNames();

        public void PopulateLightSetMappingNames()
        {
            LightSetMapping[] previousMappings = this.lightSetMappings.ToArray();
            this.lightSetMappings.Clear();

            string[] names = this.GetLightSetMappingNames();
            foreach (string name in names)
            {
                int previousIndex = previousMappings.SingleOrDefault(m => m.Name == name)?.Index ?? -1;
                this.lightSetMappings.Add(new LightSetMapping(name, previousIndex));
            }
        }

#endif

        #endregion LightSetMapping

        /// <summary>
        /// A single mapping from a well-known named set to the corresponding index in the set collection.
        /// The basic usage is for implementers to define a series of Unity-serializable integer fields and set their values in the Editor.
        /// </summary>
        [Serializable]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        public sealed class LightSetMapping
        {
            [SerializeField]
            [ReadOnly]
            private string name;

            [SerializeField]
            private int index;

            public string Name { get { return this.name; } }

            public int Index { get { return this.index; } }

            public LightSetMapping(string name, int index = -1)
            {
                this.name = name;
                this.index = index;
            }
        }
    }
}