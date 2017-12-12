using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    internal static class TransformExtensions
    {
        /// <summary>
        /// Get the position in world coordinates of a local normalized point with values between 0 and 1.
        /// </summary>
        /// <param name="rectTransform"> This RectTransform. </param>
        /// <param name="localNormalizedPoint"> The local normalized point. </param>
        /// <returns> A point in world coordinates. </returns>
        public static Vector3 GetWorldPositionOfLocalNormalizedPoint(this RectTransform rectTransform, Vector2 localNormalizedPoint)
        {
            Rect localRect = rectTransform.rect;
            Vector2 localPoint = new Vector2(
                localRect.xMin + localRect.width * localNormalizedPoint.x,
                localRect.yMin + localRect.height * localNormalizedPoint.y);
            return rectTransform.TransformPoint(localPoint);
        }
    }
}