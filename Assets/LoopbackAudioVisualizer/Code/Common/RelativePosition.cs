namespace Aleab.LoopbackAudioVisualizer.Common
{
    /// <summary>
    /// Defines the position of an object (SELF) relative to some other object (OTHER)
    /// </summary>
    public enum RelativePosition
    {
        /// <summary> SELF is at the top of OTHER. </summary>
        Top,

        /// <summary> SELF is at the right of OTHER. </summary>
        Right,

        /// <summary> SELF is at the bottom of OTHER. </summary>
        Bottom,

        /// <summary> SELF is at the left of OTHER. </summary>
        Left
    }
}