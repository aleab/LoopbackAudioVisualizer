using Aleab.LoopbackAudioVisualizer.Events;
using Aleab.LoopbackAudioVisualizer.Helpers;
using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Scripts.UI
{
    public class SettingsPanel : ShowHideComponent
    {
        /// <summary>
        /// List of devices used to populate the dropdown's options.
        /// </summary>
        private List<MMDevice> devices = new List<MMDevice>();

        #region Inspector

#pragma warning disable 0649

        [Space(10.0f)]
        [SerializeField]
        private TMP_Dropdown dropdownLoopbackDevice;

#pragma warning restore 0649

        #endregion Inspector

        #region Events

        public event EventHandler<MMDeviceEventArgs> LoopbackDeviceSelected;

        #endregion Events

        protected override void Awake()
        {
            base.Awake();
            this.RequireField(nameof(this.dropdownLoopbackDevice), this.dropdownLoopbackDevice);
        }

        private void PopulateDropdownLoopbackDevice()
        {
            MMDeviceCollection deviceCollection = MMDeviceEnumerator.EnumerateDevices(DataFlow.Render, DeviceState.Active);

            this.devices = deviceCollection.ToList();
            this.devices.Insert(0, null);

            this.dropdownLoopbackDevice.options = new List<TMP_Dropdown.OptionData>(this.devices.Count);
            foreach (var device in this.devices)
                this.dropdownLoopbackDevice.options.Add(new TMP_Dropdown.OptionData(device?.FriendlyName ?? "NONE"));

            // Select NONE or the preferred device
            int selectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(Preferences.LoopbackDeviceID))
            {
                int preferredDeviceIndex = this.devices.FindIndex(device => device?.DeviceID == Preferences.LoopbackDeviceID);
                selectedIndex = preferredDeviceIndex > 0 ? preferredDeviceIndex : 0;
            }
            this.dropdownLoopbackDevice.value = selectedIndex;
            this.dropdownLoopbackDevice.captionText.text = this.dropdownLoopbackDevice.options[selectedIndex].text;
        }

        protected override void OnShown()
        {
            base.OnShown();
            this.PopulateDropdownLoopbackDevice();
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            this.devices.Clear();
        }

        #region UI event handlers

        /* Event handlers assigned from the Unity Inspector */

        public void DropdownLoopbackDevice_ValueChanged(int selectedIndex)
        {
            MMDevice selectedDevice = this.devices[selectedIndex];
            Preferences.LoopbackDeviceID = selectedDevice?.DeviceID;
            this.OnLoopbackDeviceSelected(selectedDevice);
        }

        #endregion UI event handlers

        #region Private event handlers

        private void OnLoopbackDeviceSelected(MMDevice device)
        {
            this.LoopbackDeviceSelected?.Invoke(this, new MMDeviceEventArgs(device));
        }

        #endregion Private event handlers
    }
}