using Aleab.LoopbackAudioVisualizer.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aleab.LoopbackAudioVisualizer.Scripts.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Image), typeof(Button))]
    public class TabMenuTab : UIBehaviour, IPointerClickHandler
    {
        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private TMP_Text text;

#pragma warning restore 0649

        #endregion Inspector

        private Image image;
        private Button button;

        public RectTransform RectTransform { get; private set; }

        public Sprite Sprite
        {
            get { return this.image.sprite; }
            set { this.image.sprite = value; }
        }

        public Color Color
        {
            get { return this.image.color; }
            set { this.image.color = value; }
        }

        public Color HighlightedColor
        {
            get { return this.button.colors.normalColor; }
            set
            {
                this.button.colors = new ColorBlock
                {
                    fadeDuration = this.button.colors.fadeDuration,
                    normalColor = this.button.colors.normalColor,
                    highlightedColor = value,
                    pressedColor = this.button.colors.pressedColor,
                    disabledColor = this.button.colors.disabledColor,
                    colorMultiplier = this.button.colors.colorMultiplier
                };
            }
        }

        public event EventHandler Resized;

        public event EventHandler<PointerEventData> Clicked;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            this.RequireField(nameof(this.text), this.text);

            this.RectTransform = this.GetComponent<RectTransform>();
            this.image = this.GetComponent<Image>();
            this.button = this.GetComponent<Button>();
        }

        public void SetText(string text)
        {
            this.text.SetText(text);
            this.StartCoroutine(this.AdjustSize());
        }

        private IEnumerator AdjustSize()
        {
            yield return new WaitForEndOfFrame();

            RectTransform rectTransform = (RectTransform)this.gameObject.transform;
            float finalWidth = this.text.preferredWidth > 220.0f ? 220.0f : this.text.preferredWidth;
            rectTransform.sizeDelta = new Vector2(finalWidth, rectTransform.sizeDelta.y);
            yield return new WaitForEndOfFrame();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, this.text.preferredHeight);

            this.Resized?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            this.Clicked?.Invoke(this, eventData);
        }
    }
}