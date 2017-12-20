using Aleab.LoopbackAudioVisualizer.Common;
using Aleab.LoopbackAudioVisualizer.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.Scripts.UI
{
    public class ShowTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Coroutine showTooltipCoroutine;

        private Tooltip tooltip;

        private IEnumerable<IPointerClickHandler> parent_pointerClick;
        private IEnumerable<IPointerDownHandler> parent_pointerDown;
        private IEnumerable<IPointerUpHandler> parent_pointerUp;

        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private Tooltip tooltipPrefab;

#if UNITY_EDITOR

        public bool useTextSource;

#endif

        [SerializeField]
        [TextArea]
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

        [SerializeField]
        private bool alwaysRecreateTooltip;

#pragma warning restore 0649

        #endregion Inspector

        private void Awake()
        {
            this.RequireField(nameof(this.tooltipPrefab), this.tooltipPrefab);

            var parentGameObject = this.gameObject.transform.parent?.gameObject;
            if (parentGameObject != null)
            {
                var parentComponents = parentGameObject.GetComponents<Component>();
                this.parent_pointerClick = parentComponents.Where(component => (component as IPointerClickHandler) != null).Cast<IPointerClickHandler>();
                this.parent_pointerDown = parentComponents.Where(component => (component as IPointerDownHandler) != null).Cast<IPointerDownHandler>();
                this.parent_pointerUp = parentComponents.Where(component => (component as IPointerUpHandler) != null).Cast<IPointerUpHandler>();
            }
        }

        public void ShowTooltip()
        {
            this.showTooltipCoroutine = this.StartCoroutine(this.ShowTooltipCoroutine());
        }

        public void CancelShowTooltipOrHide()
        {
            if (this.showTooltipCoroutine != null)
                this.StopCoroutine(this.showTooltipCoroutine);
            this.showTooltipCoroutine = null;

            this.tooltip?.FadeOut(this.fadeDurationMilliseconds / 1000.0f);
            if (this.alwaysRecreateTooltip)
                this.Invoke(this.DestroyTooltip, this.fadeDurationMilliseconds / 1000.0f);
        }

        private IEnumerator ShowTooltipCoroutine()
        {
            if (this.tooltip == null)
                this.tooltip = TooltipFactory.Instance.CreateTooltip(this.tooltipPrefab, this.gameObject);

            this.RefreshTooltip();

            int remainingDelay = this.delayMilliseconds;
            while (remainingDelay > 0)
            {
                remainingDelay -= 100;
                yield return new WaitForSecondsRealtime(remainingDelay >= 100 ? 0.1f : remainingDelay / 1000.0f);
            }
            this.tooltip?.FadeIn(this.fadeDurationMilliseconds / 1000.0f);
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

        private void DestroyTooltip()
        {
            if (this.tooltip != null)
            {
                Destroy(this.tooltip.gameObject);
                this.tooltip = null;
            }
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

        #region Pointer events

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.CancelShowTooltipOrHide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Propagate the PointerClick event to the parent's components
            if (this.parent_pointerClick != null && this.parent_pointerClick.Any())
            {
                foreach (var pointerClickHandler in this.parent_pointerClick)
                    pointerClickHandler.OnPointerClick(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Propagate the PointerDown event to the parent's components
            if (this.parent_pointerDown != null && this.parent_pointerDown.Any())
            {
                foreach (var pointerDownHandler in this.parent_pointerDown)
                    pointerDownHandler.OnPointerDown(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Propagate the PointerUp event to the parent's components
            if (this.parent_pointerUp != null && this.parent_pointerUp.Any())
            {
                foreach (var pointerUpHandler in this.parent_pointerUp)
                    pointerUpHandler.OnPointerUp(eventData);
            }
        }

        #endregion Pointer events
    }
}