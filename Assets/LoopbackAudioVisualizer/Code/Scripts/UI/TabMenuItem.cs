using UnityEngine;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TabMenuItem : UIBehaviour
    {
        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        private string tabName;

#pragma warning restore 0649

        #endregion Inspector

        public string TabName { get { return this.tabName; } }
    }
}