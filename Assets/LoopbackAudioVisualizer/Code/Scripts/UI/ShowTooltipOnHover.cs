using Aleab.LoopbackAudioVisualizer.Common;
using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.UI
{
    public class ShowTooltipOnHover : MonoBehaviour
    {
        private Coroutine showTooltipCoroutine;

        private Tooltip tooltip;

        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private Tooltip tooltipPrefab;

#if UNITY_EDITOR

        [Header("Text")]
        public bool useTextSource;

#endif

        [SerializeField]
        private string text = string.Empty;

        [SerializeField]
        private TMP_Text textSource;

        [SerializeField]
        private RelativePosition position = RelativePosition.Bottom;

        [SerializeField]
        [Range(100, 5000)]
        private int delayMilliseconds = 850;

        [SerializeField]
        [Range(0.0f, 1000.0f)]
        private int fadeDurationMilliseconds = 250;

#pragma warning restore 0649

        #endregion Inspector

        private void Awake()
        {
            this.RequireField(nameof(this.tooltipPrefab), this.tooltipPrefab);

            // Create pointer event triggers
            EventTrigger eventTrigger = this.GetComponent<EventTrigger>() ?? this.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerEnterEntry.callback.AddListener(e => this.ShowTooltip());

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerExitEntry.callback.AddListener(e => this.Cancel());

            eventTrigger.triggers.Add(pointerEnterEntry);
            eventTrigger.triggers.Add(pointerExitEntry);
        }

        public void ShowTooltip()
        {
            this.showTooltipCoroutine = this.StartCoroutine(this.ShowTooltipCoroutine());
        }

        private IEnumerator ShowTooltipCoroutine()
        {
            if (this.tooltip == null)
                this.CreateTooltip();

            this.RefreshTooltip();

            int remainingDelay = this.delayMilliseconds;
            while (remainingDelay > 0)
            {
                remainingDelay -= 100;
                yield return new WaitForSecondsRealtime(remainingDelay >= 100 ? 0.1f : remainingDelay / 1000.0f);
            }
            this.tooltip.FadeIn(this.fadeDurationMilliseconds / 1000.0f);
        }

        public void Cancel()
        {
            if (this.showTooltipCoroutine != null)
                this.StopCoroutine(this.showTooltipCoroutine);
            this.showTooltipCoroutine = null;
            this.tooltip.FadeOut(this.fadeDurationMilliseconds / 1000.0f);
        }

        private void CreateTooltip()
        {
            GameObject tooltipGameObject = Instantiate(this.tooltipPrefab.gameObject, UIController.Canvas.gameObject.transform);
            tooltipGameObject.SetActive(false);
            tooltipGameObject.name = $"{this.gameObject.name} (tooltip)";

            this.tooltip = tooltipGameObject.GetComponent<Tooltip>();
        }

        private void RefreshTooltip()
        {
            if (this.tooltip == null)
                return;

            // Text
            this.SetTextFromTextSource();
            this.tooltip.SetText(this.text);

            // Position
            RectTransform thisRectTransform = (RectTransform)this.gameObject.transform;
            RectTransform rectTransform = (RectTransform)this.tooltip.gameObject.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = this.position.ToUnitySelfPivotPoint();
            rectTransform.position = thisRectTransform.GetWorldPositionOfLocalNormalizedPoint(this.position.ToUnityOtherPivotPoint());
        }

        /// <summary>
        /// Refresh <i>text</i> using <i>textSource</i> if available
        /// </summary>
        /// <returns> <code>true</code> if <i>textSource</i> is not null; otherwise <code>false</code>. </returns>
        public bool SetTextFromTextSource()
        {
            if (this.textSource != null)
            {
                this.text = this.textSource.text;
                this.tooltip?.SetText(this.text);
                return true;
            }
            return false;
        }
    }
}