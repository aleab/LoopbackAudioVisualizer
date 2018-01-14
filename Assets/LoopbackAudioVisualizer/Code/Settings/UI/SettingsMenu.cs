using Aleab.LoopbackAudioVisualizer.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace Aleab.LoopbackAudioVisualizer.Settings.UI
{
    public class SettingsMenu : ShowHideComponent
    {
        private readonly Dictionary<Type, ISettingsPanel> settingsPanels = new Dictionary<Type, ISettingsPanel>();

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