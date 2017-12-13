using Aleab.LoopbackAudioVisualizer.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class TooltipFactory : MonoBehaviour
    {
        #region Singleton

        private static TooltipFactory _instance;

        public static TooltipFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(TooltipFactory)}");
                    _instance = go.AddComponent<TooltipFactory>();

                    Debug.LogWarning($"{nameof(TooltipFactory)} object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        private readonly HashSet<Tooltip> tooltips = new HashSet<Tooltip>();

        private Coroutine cleanupCoroutine;

        #region Inspector

        [SerializeField]
        [Range(0.5f, 2.0f)]
        private float cleanupFrequency = 0.75f;

        #endregion Inspector

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            this.StartCleanUp();
        }

        #region CleanUp

        public void StartCleanUp()
        {
            if (this.cleanupCoroutine == null)
                this.cleanupCoroutine = this.StartCoroutine(this.CleanUpCoroutine());
        }

        public void StopCleanUp()
        {
            if (this.cleanupCoroutine != null)
            {
                this.StopCoroutine(this.cleanupCoroutine);
                this.cleanupCoroutine = null;
            }
        }

        private IEnumerator CleanUpCoroutine()
        {
            // StartCleanUp needs to return once before `cleanupCoroutine` gets assigned
            yield return null;

            while (this.cleanupCoroutine != null)
            {
                List<Tooltip> toDestroy = new List<Tooltip>();
                this.tooltips.RemoveWhere(tooltip =>
                {
                    if (tooltip == null)
                        return false;
                    if (tooltip.Owner == null)
                    {
                        toDestroy.Add(tooltip);
                        return true;
                    }
                    return false;
                });
                toDestroy.ForEach(tooltip => Destroy(tooltip.gameObject));

                yield return new WaitForSeconds(this.cleanupFrequency);
            }
        }

        #endregion CleanUp

        public Tooltip CreateTooltip(Tooltip template, GameObject owner)
        {
            GameObject tooltipGameObject = Instantiate(template.gameObject, UIController.Canvas.gameObject.transform);
            tooltipGameObject.SetActive(false);
            tooltipGameObject.name = $"{owner.name} (tooltip)";

            Tooltip tooltip = tooltipGameObject.GetComponent<Tooltip>();
            tooltip.Owner = owner;

            if (!this.tooltips.Add(tooltip))
                Debug.LogWarning($"[{nameof(TooltipFactory)}] Couldn't add tooltip to registered tooltips collection. ({tooltip.gameObject?.name})");

            return tooltip;
        }
    }
}