using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Aleab.LoopbackAudioVisualizer.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ShowHideComponent : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        #region Inspector

#pragma warning disable 0649

        [Space(5.0f), Header("Events")]
        [SerializeField]
        private UnityEvent Shown;

        [SerializeField]
        private UnityEvent Hidden;

#pragma warning restore 0649

        #endregion Inspector

        protected virtual void Awake()
        {
            this.canvasGroup = this.GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            this.OnShown();
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            this.OnHidden();
        }

        public void ToggleVisibility()
        {
            if (this.gameObject.activeSelf)
                this.Hide();
            else
                this.Show();
        }

        public void FadeIn(float durationSeconds)
        {
            // Enable this component in the hierarchy and hide the canvas group
            this.Show();
            this.canvasGroup.alpha = 0.0f;

            this.StartCoroutine(this.FadeCoroutine(durationSeconds, true));
        }

        public void FadeOut(float durationSeconds)
        {
            if (this.isActiveAndEnabled)
                this.StartCoroutine(this.FadeCoroutine(durationSeconds, false));
        }

        private IEnumerator FadeCoroutine(float durationSeconds, bool fadeIn)
        {
            if (durationSeconds <= 0.0f)
            {
                if (fadeIn)
                    this.Show();
                else
                    this.Hide();

                yield break;
            }

            float elapsedSeconds = 0.0f;

            if (fadeIn)
            {
                while (elapsedSeconds < durationSeconds)
                {
                    this.canvasGroup.alpha = Mathf.Clamp01(elapsedSeconds / durationSeconds);
                    yield return null;
                    elapsedSeconds += Time.deltaTime;
                }
            }
            else
            {
                while (elapsedSeconds < durationSeconds)
                {
                    this.canvasGroup.alpha = 1.0f - Mathf.Clamp01(elapsedSeconds / durationSeconds);
                    yield return null;
                    elapsedSeconds += Time.deltaTime;
                }

                // Disable this component and reset the canvas group's alpha
                this.Hide();
                this.canvasGroup.alpha = 1.0f;
            }
        }

        public void ToggleVisibility(float fadeDurationSeconds)
        {
            if (this.gameObject.activeSelf)
                this.FadeOut(fadeDurationSeconds);
            else
                this.FadeIn(fadeDurationSeconds);
        }

        protected virtual void OnShown()
        {
            this.Shown?.Invoke();
        }

        protected virtual void OnHidden()
        {
            this.Hidden?.Invoke();
        }
    }
}