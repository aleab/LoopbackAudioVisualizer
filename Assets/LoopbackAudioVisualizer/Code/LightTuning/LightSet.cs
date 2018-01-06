using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    [Serializable]
    public class LightSet
    {
        private Dictionary<TuningParameters, ILightSetTuning> _lightSetTunings;

        #region Serialized Fields

        [SerializeField]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private bool relativeTuning;

        [SerializeField]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private Light[] lights;

        [SerializeField]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private TuningParameters[] tunings;

        #endregion Serialized Fields

        private bool initialized;

        private List<LightValues> defaultLightValues;

        #region Accessors

        public bool RelativeTuning { get { return this.relativeTuning; } }

        public IReadOnlyCollection<Light> Lights { get { return this.lights; } }

        public IReadOnlyCollection<TuningParameters> Tunings { get { return this.tunings; } }

        public Dictionary<TuningParameters, ILightSetTuning> LightSetTunings
        {
            get { return this._lightSetTunings ?? (this._lightSetTunings = new Dictionary<TuningParameters, ILightSetTuning>(new TuningParametersComparer())); }
        }

        #endregion Accessors

        public LightSet(Light[] lights, TuningParameters[] tunings, bool relativeTuning = false)
        {
            this.relativeTuning = relativeTuning;
            this.lights = lights;
            this.tunings = tunings;
            this.Init();
        }

        private void Init()
        {
            this.ReplaceDefaultLightValues();
            this.CreateLightSetTunings();
            this.initialized = true;
        }

        private void RequireInitialization()
        {
            if (!this.initialized)
                this.Init();
#if UNITY_EDITOR
            else if (EditorApplication.isPlaying)
            {
                // TODO: Performace-heavy? Is it better to move these to the PropertyDrawer?
                this.CleanAndUpdateDefaultLightValues();
                this.CleanAndUpdateLightSetTunings();
            }
#endif
        }

        public void ApplyTunings()
        {
            if (this.lights != null)
            {
                this.RequireInitialization();

                foreach (var kvp in this.LightSetTunings)
                {
                    var tuning = kvp.Key;
                    var lightSetTuning = kvp.Value;
                    foreach (var light in this.lights)
                    {
                        if (this.relativeTuning)
                        {
                            object defaultValue = tuning.TuningTarget.GetBoxedPropertyValue(this.GetDefaultValues(light));
                            lightSetTuning.Tune(light, tuning.TuningTarget, defaultValue);
                        }
                        else
                            lightSetTuning.Tune(light, tuning.TuningTarget);
                    }
                }
            }
        }

        public LightValues GetDefaultValues(Light light)
        {
            return this.defaultLightValues?.SingleOrDefault(l => l.Light?.GetInstanceID() == light.GetInstanceID());
        }

        #region ResetDefault*

        public void ResetDefaultValues()
        {
            if (this.lights != null)
            {
                foreach (var light in this.lights)
                {
                    LightValues defaultValues = this.defaultLightValues?.SingleOrDefault(l => l.Light?.GetInstanceID() == light.GetInstanceID());
                    if (defaultValues != null)
                    {
                        light.range = defaultValues.Range;
                        light.intensity = defaultValues.Intensity;
                        light.color = defaultValues.Color;
                    }
                }
            }
        }

        public void ResetDefaultRanges()
        {
            if (this.lights != null)
            {
                foreach (var light in this.lights)
                {
                    LightValues defaultValues = this.defaultLightValues?.SingleOrDefault(l => l.Light?.GetInstanceID() == light.GetInstanceID());
                    if (defaultValues != null)
                        light.range = defaultValues.Range;
                }
            }
        }

        public void ResetDefaultIntensities()
        {
            if (this.lights != null)
            {
                foreach (var light in this.lights)
                {
                    LightValues defaultValues = this.defaultLightValues?.SingleOrDefault(l => l.Light?.GetInstanceID() == light.GetInstanceID());
                    if (defaultValues != null)
                        light.intensity = defaultValues.Intensity;
                }
            }
        }

        public void ResetDefaultColors()
        {
            if (this.lights != null)
            {
                foreach (var light in this.lights)
                {
                    LightValues defaultValues = this.defaultLightValues?.SingleOrDefault(l => l.Light?.GetInstanceID() == light.GetInstanceID());
                    if (defaultValues != null)
                        light.color = defaultValues.Color;
                }
            }
        }

        #endregion ResetDefault*

        #region LightSetTunings

        private void ClearLightSetTunings()
        {
            foreach (var kvp in this.LightSetTunings)
                kvp.Key.InternalTuningParametersChanged -= this.TuningParameters_InternalTuningParametersChanged;
            this.LightSetTunings.Clear();
        }

        private void CreateLightSetTunings()
        {
            this.ClearLightSetTunings();
            if (this.tunings != null)
            {
                foreach (var tuning in this.tunings)
                {
                    this.LightSetTunings[tuning] = LightSetTuningFactory.CreateLightSetTuning(tuning);
                    tuning.InternalTuningParametersChanged -= this.TuningParameters_InternalTuningParametersChanged;
                    tuning.InternalTuningParametersChanged += this.TuningParameters_InternalTuningParametersChanged;
                }
            }
        }

        private void CleanAndUpdateLightSetTunings()
        {
            if (this.tunings != null)
            {
                if (this.LightSetTunings.Count == 0 && this.tunings.Length > 0)
                    this.CreateLightSetTunings();
                else
                {
                    // Add new tunings to LightSetTunings
                    foreach (var tuning in this.tunings)
                    {
                        if (!this.LightSetTunings.ContainsKey(tuning))
                            this.LightSetTunings[tuning] = LightSetTuningFactory.CreateLightSetTuning(tuning);
                    }

                    // Remove old tunings from LightSetTunings
                    var keys = this.LightSetTunings.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        if (this.tunings.Count(tuning => tuning.Equals(key)) == 0)
                        {
                            key.InternalTuningParametersChanged -= this.TuningParameters_InternalTuningParametersChanged;
                            this.LightSetTunings.Remove(key);
                        }
                    }
                }
            }
            else
                this.ClearLightSetTunings();
        }

        private void TuningParameters_InternalTuningParametersChanged(object sender, InternalTuningParametersChangedEventArgs e)
        {
            TuningParameters tuning = e.TuningParameters;
            if (tuning == null)
                return;

            // Re-create the LightSetTuning object
            if (this.LightSetTunings.ContainsKey(tuning))
                this.LightSetTunings[tuning] = LightSetTuningFactory.CreateLightSetTuning(tuning);
        }

        #endregion LightSetTunings

        #region DefaultLightValues

        private void ReplaceDefaultLightValues()
        {
            this.defaultLightValues?.Clear();

            if (this.lights != null)
            {
                this.defaultLightValues = new List<LightValues>(this.lights.Length);
                foreach (Light light in this.lights)
                    this.defaultLightValues.Add(new LightValues(light));
            }
        }

        /// <summary>
        /// Removes <see cref="LightValues"/> instances of old, removed lights or duplicates and adds <see cref="LightValues"/> of new, untracked lights.
        /// </summary>
        private void CleanAndUpdateDefaultLightValues()
        {
            if (this.lights != null)
            {
                if (this.defaultLightValues == null)
                    this.ReplaceDefaultLightValues();
                else
                {
                    // Add new lights to defaultLightValues
                    foreach (var light in this.lights)
                    {
                        var lightValues = this.defaultLightValues.Where(l => l.Light?.GetInstanceID() == light.GetInstanceID()).ToList();
                        if (lightValues.Count == 0)
                            this.defaultLightValues.Add(new LightValues(light));
                        else if (lightValues.Count > 1)
                        {
                            // Remove duplicates
                            while (this.defaultLightValues.Count(l => l.Light?.GetInstanceID() == light.GetInstanceID()) > 1)
                                this.defaultLightValues.RemoveAt(this.defaultLightValues.FindLastIndex(l => l.Light?.GetInstanceID() == light.GetInstanceID()));
                        }
                    }

                    // Remove old lights from defaultLightValues
                    for (int i = 0; i < this.defaultLightValues.Count; ++i)
                    {
                        var lightValues = this.defaultLightValues[i];
                        if (lightValues.Light == null || this.lights.Count(light => light?.GetInstanceID() == lightValues.Light.GetInstanceID()) == 0)
                        {
                            // Reset to default before removing
                            if (lightValues.Light != null)
                            {
                                lightValues.Light.range = lightValues.Range;
                                lightValues.Light.intensity = lightValues.Intensity;
                                lightValues.Light.color = lightValues.Color;
                            }
                            this.defaultLightValues.RemoveAt(i--);
                        }
                    }
                }
            }
            else
                this.defaultLightValues?.Clear();
        }

        #endregion DefaultLightValues
    }
}