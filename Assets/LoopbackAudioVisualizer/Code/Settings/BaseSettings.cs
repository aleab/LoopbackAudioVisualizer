using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using DefaultValue = System.ComponentModel.DefaultValueAttribute;
using ReadOnly = Aleab.LoopbackAudioVisualizer.Unity.ReadOnlyAttribute;

namespace Aleab.LoopbackAudioVisualizer.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseSettings : ScriptableObject, IEquatable<BaseSettings>
    {
        #region Inspector

        [SerializeField]
        [ReadOnly]
        [JsonProperty, DefaultValue(false)]
        private bool isPreset = true;

        #endregion Inspector

        protected int? hashCode;
        protected bool dirty;

        public bool IsPreset
        {
            get { return this.isPreset; }
            private set { this.isPreset = value; }
        }

        public string FilePath { get { return GetSettingsFilePath(this.GetType().Name); } }

        /// <summary>
        /// Set a property's value.
        /// </summary>
        /// <typeparam name="T"> The type of the property. </typeparam>
        /// <param name="field"> A reference to the property's backing field. </param>
        /// <param name="value"> The new value. </param>
        /// <param name="setDirty"> Whether this change should mark the settings object as dirty. </param>
        /// <param name="save"> Whether to save the settings or not after changing the value. </param>
        /// <returns> True if the value has been changed. If <see cref="IsPreset"/> is true, the value will not be changed. </returns>
        protected bool SetProperty<T>(ref T field, T value, bool setDirty = true, bool save = false) where T : IEquatable<T>
        {
            if (this.IsPreset)
                return false;
            if (ReferenceEquals(field, value) || (field?.Equals(value) ?? value == null))
                return false;

            field = value;
            this.dirty |= setDirty;
            if (save)
                this.Save();
            return true;
        }

        public void Save()
        {
            if (!this.dirty && File.Exists(this.FilePath))
                return;

            BaseSettings clone = (BaseSettings)this.MemberwiseClone();
            clone.IsPreset = false;

            string json = JsonConvert.SerializeObject(clone, JsonSerializerSettings);

            DirectoryInfo parentDir = Directory.GetParent(this.FilePath);
            if (parentDir != null && !parentDir.Exists)
                Directory.CreateDirectory(parentDir.FullName);
            File.WriteAllText(this.FilePath, json, Encoding.UTF8);

            this.dirty = false;
        }

        public void Save(bool recursive)
        {
            this.Save();
            if (recursive)
            {
                var subSettings = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                .Where(f => f.FieldType.IsSubclassOf(typeof(BaseSettings)) && (f.IsPublic || f.GetCustomAttributes<SerializeField>().Any()))
                                                .Select(f => f.GetValue(this) as BaseSettings);

                foreach (var subSetting in subSettings)
                    subSetting?.Save(true);
            }
        }

        #region Static

        /// <summary>
        /// The application's Settings folder.
        /// </summary>
        public static string SettingsFolderPath { get { return Path.Combine(AppSettings.AppData, "Settings"); } }

        /// <summary>
        /// The file extension used for settings files.
        /// </summary>
        public static readonly string SettingsFileExtension = "json";

        /// <summary>
        /// Settings used to (de-)serialize <see cref="BaseSettings"/> objects.
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings { get; }

        static BaseSettings()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.Default,
                FloatParseHandling = FloatParseHandling.Decimal,
                FloatFormatHandling = FloatFormatHandling.String,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.Indented,
                MaxDepth = null,
                Culture = CultureInfo.InvariantCulture,
                ConstructorHandling = ConstructorHandling.Default,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                TypeNameHandling = TypeNameHandling.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        /// <summary>
        /// Combines <see cref="SettingsFolderPath"/> and the given file name.
        /// </summary>
        /// <param name="settingsFileName"> A settings file name, without extension. </param>
        /// <returns> The combined file path. </returns>
        public static string GetSettingsFilePath(string settingsFileName)
        {
            return Path.Combine(SettingsFolderPath, $"{settingsFileName}.{SettingsFileExtension}");
        }

        /// <summary>
        /// Get the file path of the given type of settings.
        /// </summary>
        /// <typeparam name="T"> The type of settings. </typeparam>
        /// <returns> The settings file path. </returns>
        public static string GetSettingsFilePath<T>() where T : BaseSettings
        {
            return GetSettingsFilePath(typeof(T).Name);
        }

        /// <summary>
        /// Check if a settings file exists for the given type of settings.
        /// </summary>
        /// <typeparam name="T"> The type of settings. </typeparam>
        /// <returns> Whether the settings file exists or not. </returns>
        public static bool Exists<T>() where T : BaseSettings
        {
            return File.Exists(GetSettingsFilePath<T>());
        }

        /// <summary>
        /// Deserializes the settings file of the given type of settings, if it exists.
        /// </summary>
        /// <typeparam name="T"> The type of settings. </typeparam>
        /// <returns> The deserialized settings, or null. </returns>
        public static T GetSavedSettings<T>() where T : BaseSettings
        {
            return GetSavedSettings<T>(null);
        }

        /// <summary>
        /// Deserializes the settings file of the given type of settings, if it exists.
        /// </summary>
        /// <typeparam name="T"> The type of settings. </typeparam>
        /// <param name="defaultFallback"> The default value to return if the saved settings don't exist. </param>
        /// <returns> The deserialized settings, or null. </returns>
        public static T GetSavedSettings<T>(T defaultFallback) where T : BaseSettings
        {
            if (Exists<T>())
            {
                string json = File.ReadAllText(GetSettingsFilePath<T>());
                T settings = CreateInstance<T>();
                JsonConvert.PopulateObject(json, settings, JsonSerializerSettings);
                return settings;
            }
            return defaultFallback;
        }

        #endregion Static

        #region Equals, GetHashCode

        /// <inheritdoc />
        public bool Equals(BaseSettings other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return this.isPreset == other.isPreset;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            BaseSettings that = obj as BaseSettings;
            return this.Equals(that);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
                this.hashCode = unchecked(this.isPreset.GetHashCode() * 397);
            return this.hashCode.Value;
        }

        #endregion Equals, GetHashCode
    }
}