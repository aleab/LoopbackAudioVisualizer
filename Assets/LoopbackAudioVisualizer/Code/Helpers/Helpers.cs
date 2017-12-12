using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    internal static class Helpers
    {
        public static int ToNearestPowerOfTwo(int n)
        {
            return (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(n) / Mathf.Log(2)));
        }
    }
}