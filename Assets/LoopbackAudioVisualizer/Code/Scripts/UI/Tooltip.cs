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

        private GameObject _owner;

        /// <summary>
        /// The owner of the tooltip
        /// </summary>
        /// <remarks> Can only be set once! </remarks>
        /// <exception cref="SetOncePropertyAlreadySetException"> if the property's set accessor is used after already setting a first value. </exception>
        public GameObject Owner
        {
            get { return this._owner; }
            set
            {
                if (this._owner == null)
                    this._owner = value;
                else
                    throw new SetOncePropertyAlreadySetException(nameof(this.Owner));
            }
        }

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

        public override bool Equals(object other)
        {
            Tooltip that = other as Tooltip;
            if (that == null)
                return false;

            return (this.text != null && this.text.Equals(that.text) || this.text == null && that.text == null) &&
                   this.maxWidth == that.maxWidth &&
                   (this._owner != null && this._owner.Equals(that._owner) || this._owner == null && that._owner == null);
        }

        public override int GetHashCode()
        {
            int k = 407;
            return (this.text?.GetHashCode() ?? (k = k * k)) ^ (this.maxWidth + k) ^ (this._owner?.GetHashCode() ?? (k * k));
        }
    }
}