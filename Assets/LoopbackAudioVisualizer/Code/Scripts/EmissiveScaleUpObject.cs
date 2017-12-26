using Aleab.LoopbackAudioVisualizer.Common;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using UnityEditor;
using UnityEngine;
using Range = Aleab.LoopbackAudioVisualizer.Unity.RangeAttribute;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    [ExecuteInEditMode]
    public class EmissiveScaleUpObject : ScaleUpObject
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        [DisableWhenPlaying]
        private bool useSharedMaterial;

        [SerializeField]
        [DisableWhenPlaying]
        private Light bottomLight;

        [SerializeField]
        [Range(0.0f, 5.0f)]
        private float minLightIntensity, maxLightIntensity;

#pragma warning restore 0414, 0649

        #endregion Inspector

        private Material material;

        private Color baseColor, emissionColor;

        private float bottomLightOriginalRange;

        private float bottomLightOriginalIntensity;

        public bool UseSharedMaterial { get { return this.useSharedMaterial; } }

        public Color[] CurrentColors { get { return new[] { this.baseColor, this.emissionColor }; } }

        protected override void Awake()
        {
            base.Awake();

            this.RequireField(nameof(this.bottomLight), this.bottomLight);

            // Lights
            this.bottomLightOriginalRange = this.bottomLight.range;
            this.bottomLightOriginalIntensity = this.bottomLight.intensity;

#if UNITY_EDITOR
            // If [ExecuteInEditMode], stop here: no need (categorically, NONE!) to check for the material.
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            // Material
            this.material = this.useSharedMaterial ? this.MeshRenderer.sharedMaterial : this.MeshRenderer.materials[0];
            this.material.EnableKeyword("_EMISSION");
        }

        protected virtual void Start()
        {
#if UNITY_EDITOR
            // If [ExecuteInEditMode]
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                this.StartForEditMode();
                return;
            }
#endif

            // Colors
            this.baseColor = this.material.color;
            this.emissionColor = this.material.GetColor("_EmissionColor");

            // Lights
            double currentInitialScalesRatio = (double)Mathf.Max(this.gameObject.transform.localScale.x, this.gameObject.transform.localScale.z) / Mathf.Max(this.initialScale.x, this.initialScale.z);
            this.bottomLight.color = Color.Lerp(this.baseColor, this.emissionColor, 0.75f);
            this.bottomLight.range = (float)(this.bottomLightOriginalRange * currentInitialScalesRatio);
            this.UpdateBottomLightPosition();
        }

        public void SetBaseColor(Color newColor) => this.SetColor(newColor, ref this.baseColor, true);

        public void SetBaseColor(HSVChannel hsvChannel, float channelValue) => this.SetColor(hsvChannel, channelValue, ref this.baseColor, true);

        public void SetEmissionColor(Color newColor) => this.SetColor(newColor, ref this.emissionColor, false);

        public void SetEmissionColor(HSVChannel hsvChannel, float channelValue) => this.SetColor(hsvChannel, channelValue, ref this.emissionColor, false);

        public void SetLightsColor(Color newColor)
        {
            this.bottomLight.color = newColor;
        }

        private void SetColor(HSVChannel hsvChannel, float channelValue, ref Color currentColor, bool isBaseColor)
        {
            float H, S, V;
            Color.RGBToHSV(currentColor, out H, out S, out V);

            switch (hsvChannel)
            {
                case HSVChannel.Hue:
                    this.SetColor(Color.HSVToRGB(channelValue, S, V, !isBaseColor), ref currentColor, isBaseColor);
                    break;

                case HSVChannel.Saturation:
                    this.SetColor(Color.HSVToRGB(H, channelValue, V, !isBaseColor), ref currentColor, isBaseColor);
                    break;

                case HSVChannel.Value:
                    this.SetColor(Color.HSVToRGB(H, S, channelValue, !isBaseColor), ref currentColor, isBaseColor);
                    break;
            }
        }

        private void SetColor(Color newColor, ref Color currentColor, bool? isBaseColor = null)
        {
            currentColor.r = newColor.r;
            currentColor.g = newColor.g;
            currentColor.b = newColor.b;
            currentColor.a = newColor.a;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (isBaseColor == true)
                this.material.color = currentColor;
            else if (isBaseColor == false)
                this.material.SetColor("_EmissionColor", currentColor);
        }

        /// <summary>
        /// Set the lights intensity to a percentual value of the max possible intensity.
        /// </summary>
        /// <param name="intensity"> Percentual value (0.0f to 1.0f). </param>
        public void SetLightsIntensity(float intensity)
        {
            this.bottomLight.intensity = (this.minLightIntensity + (this.maxLightIntensity - this.minLightIntensity) * intensity) * this.bottomLightOriginalIntensity;
        }

        private void UpdateBottomLightPosition()
        {
            // Bring the bottom light up of 15% of its radius in world coordinates.
            float newY = this.gameObject.transform.InverseTransformPoint(Vector3.up * (0.15f * this.bottomLight.range)).y;
            this.bottomLight.gameObject.transform.localPosition = Vector3.up * newY;
        }

        protected override void OnScaled()
        {
            base.OnScaled();
            this.UpdateBottomLightPosition();
        }

        #region ExecuteInEditMode

#if UNITY_EDITOR

        private void StartForEditMode()
        {
            // Colors
            this.baseColor = this.MeshRenderer.sharedMaterial.color;
            this.emissionColor = this.MeshRenderer.sharedMaterial.GetColor("_EmissionColor");

            // Lights
            double currentInitialScalesRatio = (double)Mathf.Max(this.gameObject.transform.localScale.x, this.gameObject.transform.localScale.z) / Mathf.Max(this.initialScale.x, this.initialScale.z);
            this.bottomLight.color = Color.Lerp(this.baseColor, this.emissionColor, 0.75f);
            this.bottomLight.range = (float)(this.bottomLightOriginalRange * currentInitialScalesRatio);
            this.UpdateBottomLightPosition();
        }

#endif

        #endregion ExecuteInEditMode
    }
}