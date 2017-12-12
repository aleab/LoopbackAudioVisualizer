using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer
{
    internal static class Preferences
    {
        public static bool WindowsTaskbar { get; set; }

        public static string LoopbackDeviceID { get; set; }

        public static int CaptureBufferMilliseconds { get; set; } = 500;

        public static void Save()
        {
            PlayerPrefs.SetString(nameof(WindowsTaskbar), WindowsTaskbar.ToString());
            PlayerPrefs.SetString(nameof(LoopbackDeviceID), LoopbackDeviceID);
            PlayerPrefs.SetInt(nameof(CaptureBufferMilliseconds), CaptureBufferMilliseconds);

            PlayerPrefs.Save();
        }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(nameof(WindowsTaskbar)))
                WindowsTaskbar = ParseBool(PlayerPrefs.GetString(nameof(WindowsTaskbar)), false);
            if (PlayerPrefs.HasKey(nameof(LoopbackDeviceID)))
                LoopbackDeviceID = PlayerPrefs.GetString(nameof(LoopbackDeviceID));
            if (PlayerPrefs.HasKey(nameof(CaptureBufferMilliseconds)))
                CaptureBufferMilliseconds = ValidateInt(PlayerPrefs.GetInt(nameof(CaptureBufferMilliseconds)), new Vector2(500, int.MaxValue), 500);
        }

        private static bool ParseBool(string boolString, bool defaultValue)
        {
            bool result;
            return bool.TryParse(boolString, out result) ? result : defaultValue;
        }

        private static int ValidateInt(int input, Vector2 range, int defaultValue)
        {
            return (input >= range.x && input <= range.y) ? input : defaultValue;
        }
    }
}