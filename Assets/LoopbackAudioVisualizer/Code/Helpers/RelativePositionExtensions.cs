using Aleab.LoopbackAudioVisualizer.Common;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    internal static class RelativePositionExtensions
    {
        public static Vector2 ToUnitySelfPivotPoint(this RelativePosition relativePosition)
        {
            switch (relativePosition)
            {
                case RelativePosition.Top:
                    return new Vector2(0.5f, 0.0f);

                case RelativePosition.Right:
                    return new Vector2(0.0f, 0.5f);

                case RelativePosition.Bottom:
                    return new Vector2(0.5f, 1.0f);

                case RelativePosition.Left:
                    return new Vector2(1.0f, 0.5f);

                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        public static Vector2 ToUnityOtherPivotPoint(this RelativePosition relativePosition)
        {
            switch (relativePosition)
            {
                case RelativePosition.Top:
                    return new Vector2(0.5f, 1.0f);

                case RelativePosition.Right:
                    return new Vector2(1.0f, 0.5f);

                case RelativePosition.Bottom:
                    return new Vector2(0.5f, 0.0f);

                case RelativePosition.Left:
                    return new Vector2(0.0f, 0.5f);

                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }
    }
}