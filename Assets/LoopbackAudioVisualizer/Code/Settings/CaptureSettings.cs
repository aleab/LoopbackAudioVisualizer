using Aleab.LoopbackAudioVisualizer.Unity;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using DefaultValue = System.ComponentModel.DefaultValueAttribute;
using Range = Aleab.LoopbackAudioVisualizer.Unity.RangeAttribute;

namespace Aleab.LoopbackAudioVisualizer.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CaptureSettings : BaseSettings, IEquatable<CaptureSettings>
    {
        #region Inspector

        [Header("Capture Settings")]
        [SerializeField]
        [ReadOnly]
        [JsonProperty]
        private string loopbackDeviceID;

        [SerializeField]
        [Tooltip("The buffer size in milliseconds; the actual size in bytes depends on the WaveFormat.")]
        [Range(100.0f, 2000.0f)]
        [JsonProperty, DefaultValue(500)]
        private int bufferSizeMilliseconds = 500;

        #endregion Inspector

        public string LoopbackDeviceID
        {
            get { return this.loopbackDeviceID; }
            set { this.SetProperty(ref this.loopbackDeviceID, value, save: true); }
        }

        public int BufferSizeMilliseconds
        {
            get { return this.bufferSizeMilliseconds; }
            set { this.SetProperty(ref this.bufferSizeMilliseconds, value); }
        }

        #region Equals, GetHashCode

        /// <inheritdoc />
        public bool Equals(CaptureSettings other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) &&
                   string.Equals(this.loopbackDeviceID, other.loopbackDeviceID) &&
                   this.bufferSizeMilliseconds == other.bufferSizeMilliseconds;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            CaptureSettings that = obj as CaptureSettings;
            return this.Equals(that);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                if (!this.hashCode.HasValue)
                {
                    this.hashCode = base.GetHashCode();
                    this.hashCode = (this.hashCode * 397) ^ (this.loopbackDeviceID != null ? this.loopbackDeviceID.GetHashCode() : 0);
                    this.hashCode = (this.hashCode * 397) ^ this.bufferSizeMilliseconds;
                }
                return this.hashCode.Value;
            }
        }

        #endregion Equals, GetHashCode
    }
}