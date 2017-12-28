using Aleab.LoopbackAudioVisualizer.Helpers;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    [DisallowMultipleComponent]
    public class ScaleUpObject : MonoBehaviour
    {
        #region Inspector

#pragma warning disable 0414, 0649

        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        [Range(0.0f, float.MaxValue)]
        private float minimumScale = 0.05f;

        [SerializeField]
        [Range(50.0f, 100.0f)]
        private float smoothScaleDuration = 50.0f;

#pragma warning restore 0414, 0649

        #endregion Inspector

        protected Vector3 initialScale;

        private Coroutine scaleSmoothCoroutine;

        public MeshRenderer MeshRenderer { get { return this.meshRenderer; } }

        public float MinimumScale { get { return this.minimumScale; } }

        protected virtual void Awake()
        {
            this.RequireField(nameof(this.meshRenderer), this.meshRenderer);
            this.initialScale = this.gameObject.transform.localScale;
        }

        public void Scale(float scale, bool relative = false)
        {
            float absoluteScale = relative ? scale * Mathf.Max(this.initialScale.y, this.MinimumScale) : scale;
            this.gameObject.transform.localScale = new Vector3(
                this.gameObject.transform.localScale.x,
                Mathf.Abs(absoluteScale) >= this.minimumScale ? absoluteScale : this.minimumScale,
                this.gameObject.transform.localScale.z);
            this.OnScaled();
        }

        public void ScaleSmooth(float scale, bool relative = false)
        {
            float absoluteScale = relative ? scale * this.initialScale.y : scale;
            float finalYScale = Mathf.Abs(absoluteScale) >= this.minimumScale ? absoluteScale : this.minimumScale;

            if (this.scaleSmoothCoroutine != null)
                this.StopCoroutine(this.scaleSmoothCoroutine);

            this.scaleSmoothCoroutine = this.StartCoroutine(this.ScaleSmoothCoroutine(finalYScale));
        }

        private IEnumerator ScaleSmoothCoroutine(float finalYScale)
        {
            yield return null;

            float initialYScale = this.gameObject.transform.localScale.y;
            float deltaYScale = finalYScale - initialYScale;
            float remainingMilliseconds = this.smoothScaleDuration;
            while (remainingMilliseconds > 0)
            {
                this.gameObject.transform.localScale = new Vector3(
                    this.gameObject.transform.localScale.x,
                    initialYScale + deltaYScale * ((this.smoothScaleDuration - remainingMilliseconds) / this.smoothScaleDuration),
                    this.gameObject.transform.localScale.z);
                this.OnScaled();
                yield return null;

                remainingMilliseconds -= Time.deltaTime * 1000;
            }
        }

        protected virtual void OnScaled()
        {
        }
    }
}