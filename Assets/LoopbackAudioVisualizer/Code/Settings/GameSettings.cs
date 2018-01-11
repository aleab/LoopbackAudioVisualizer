using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GameSettings : BaseSettings, IEquatable<GameSettings>
    {
        #region Inspector

        [Header("Settings")]
        [SerializeField]
        private CaptureSettings captureSettings;

        #endregion Inspector

        public CaptureSettings CaptureSettings
        {
            get { return this.captureSettings; }
            set { this.SetProperty(ref this.captureSettings, value, setDirty: false); }
        }

        #region Equals, GetHashCode

        /// <inheritdoc />
        public bool Equals(GameSettings other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) &&
                   (this.captureSettings?.Equals(other.captureSettings) ?? other.captureSettings == null);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            GameSettings that = obj as GameSettings;
            return this.Equals(that);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                if (!this.hashCode.HasValue)
                    this.hashCode = (base.GetHashCode() * 397) ^
                                    (this.captureSettings != null ? this.captureSettings.GetHashCode() : 0);
                return this.hashCode.Value;
            }
        }

        #endregion Equals, GetHashCode
    }
}