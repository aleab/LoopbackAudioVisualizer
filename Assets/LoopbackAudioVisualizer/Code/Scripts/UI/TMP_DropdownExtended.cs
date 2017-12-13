using Aleab.LoopbackAudioVisualizer.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aleab.LoopbackAudioVisualizer.UI
{
    // ReSharper disable once InconsistentNaming
    public class TMP_DropdownExtended : TMP_Dropdown
    {
        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private Image arrow;

#pragma warning restore 0649

        #endregion Inspector

        /// <inheritdoc />
        protected override void Awake()
        {
            this.RequireField(nameof(this.arrow), this.arrow);
            base.Awake();

            this.onValueChanged.AddListener(index => this.RefreshArrowRotation(true));
        }

        private void RefreshArrowRotation(bool preExpandedChange = false)
        {
            int m = this.IsExpanded ? (preExpandedChange ? 0 : 1) : (preExpandedChange ? 1 : 0);
            this.arrow.gameObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f * m);
        }

        /// <inheritdoc />
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            this.RefreshArrowRotation();
        }

        /// <inheritdoc />
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            this.RefreshArrowRotation(true);
        }
    }
}