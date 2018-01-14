using UnityEngine;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.Settings.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class SettingsPanel<T> : UIBehaviour, ISettingsPanel where T : BaseSettings
    {
        protected RectTransform rectTrasform;

        protected abstract T Settings { get; }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            this.rectTrasform = this.GetComponent<RectTransform>();
        }
    }

    public interface ISettingsPanel
    {
    }
}