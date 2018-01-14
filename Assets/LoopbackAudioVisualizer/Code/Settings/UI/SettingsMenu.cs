using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.UI;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.Settings.UI
{
    public class SettingsMenu : ShowHideComponent
    {
        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        [DisableWhenPlaying]
        private TabMenu tabMenu;

#pragma warning restore 0649

        #endregion Inspector

        private readonly Dictionary<Type, ISettingsPanel> settingsPanels = new Dictionary<Type, ISettingsPanel>();

        public TabMenu TabMenu { get { return this.tabMenu; } }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            this.RequireField(nameof(this.tabMenu), this.tabMenu);
        }

        public T FindSettingsPanel<T>() where T : UIBehaviour, ISettingsPanel
        {
            var settingPanel = this.settingsPanels.ContainsKey(typeof(T)) ? this.settingsPanels[typeof(T)] : null;
            if (settingPanel == null)
            {
                settingPanel = this.GetComponentsInChildren<T>(true).SingleOrDefault();
                this.settingsPanels[typeof(T)] = settingPanel;
            }
            return (T)settingPanel;
        }
    }
}