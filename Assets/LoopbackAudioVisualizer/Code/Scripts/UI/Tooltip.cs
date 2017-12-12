using Aleab.LoopbackAudioVisualizer.Helpers;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.UI
{
    public class Tooltip : ShowHideComponent
    {
        private const int MIN_MAX_WIDTH = 100;
        private const int MAX_MAX_WIDTH = 320;

        #region Inspector

#pragma warning disable 0649

        [Space(10.0f)]
        [SerializeField]
        private TMP_Text text;

#pragma warning restore 0649

        [Space(5.0f)]
        [Range(MIN_MAX_WIDTH, MAX_MAX_WIDTH)]
        [SerializeField]
        private int maxWidth = 240;

        #endregion Inspector

        protected override void Awake()
        {
            base.Awake();
            this.RequireField(nameof(this.text), this.text);
        }

        public void SetText(string text)
        {
            this.text.SetText(text);
        }

        private IEnumerator AdjustSize()
        {
            yield return new WaitForEndOfFrame();
            if (this.text.isTextOverflowing)
            {
                RectTransform rectTransform = (RectTransform)this.gameObject.transform;
                float finalWidth = this.text.preferredWidth > this.maxWidth ? this.maxWidth : this.text.preferredWidth;
                rectTransform.sizeDelta = new Vector2(finalWidth, rectTransform.sizeDelta.y);
                yield return new WaitForEndOfFrame();
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, this.text.preferredHeight);
            }
        }

        protected override void OnShown()
        {
            this.StartCoroutine(this.AdjustSize());
            base.OnShown();
        }
    }
}