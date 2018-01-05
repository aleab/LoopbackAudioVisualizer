using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public enum TuningTarget
    {
        Range,
        Intensity,
        Color
    }

    public static class TuningTargetExtensions
    {
        public static Type GetPropertyType(this TuningTarget tuningTarget)
        {
            switch (tuningTarget)
            {
                case TuningTarget.Range:
                case TuningTarget.Intensity:
                    return typeof(float);

                case TuningTarget.Color:
                    return typeof(Color);
            }
            return null;
        }

        /// <summary>
        /// Get the object-boxed property value from the <see cref="Light"/> object corresponding to this <see cref="TuningTarget"/>.
        /// </summary>
        /// <param name="tuningTarget"> This tuning target. </param>
        /// <param name="light"> The <see cref="Light"/> object. </param>
        /// <returns> An object-boxed value. </returns>
        public static object GetBoxedPropertyValue(this TuningTarget tuningTarget, Light light)
        {
            switch (tuningTarget)
            {
                case TuningTarget.Range:
                    return light.range;

                case TuningTarget.Intensity:
                    return light.intensity;

                case TuningTarget.Color:
                    return light.color;
            }
            return null;
        }

        /// <summary>
        /// Get the object-boxed property value from the <see cref="Light"/> object corresponding to this <see cref="TuningTarget"/>.
        /// </summary>
        /// <param name="tuningTarget"> This tuning target. </param>
        /// <param name="light"> The <see cref="Light"/> object. </param>
        /// <param name="type"> The type of the fetched property. </param>
        /// <returns> An object-boxed value. </returns>
        public static object GetBoxedPropertyValue(this TuningTarget tuningTarget, Light light, out Type type)
        {
            type = tuningTarget.GetPropertyType();
            return tuningTarget.GetBoxedPropertyValue(light);
        }

        /// <summary>
        /// Get the object-boxed property value from the <see cref="LightValues"/> object corresponding to this <see cref="TuningTarget"/>.
        /// </summary>
        /// <param name="tuningTarget"> This tuning target. </param>
        /// <param name="lightValues"> The <see cref="LightValues"/> object. </param>
        /// <returns> An object-boxed value. </returns>
        public static object GetBoxedPropertyValue(this TuningTarget tuningTarget, LightValues lightValues)
        {
            switch (tuningTarget)
            {
                case TuningTarget.Range:
                    return lightValues.Range;

                case TuningTarget.Intensity:
                    return lightValues.Intensity;

                case TuningTarget.Color:
                    return lightValues.Color;
            }
            return null;
        }

        /// <summary>
        /// Get the object-boxed property value from the <see cref="LightValues"/> object corresponding to this <see cref="TuningTarget"/>.
        /// </summary>
        /// <param name="tuningTarget"> This tuning target. </param>
        /// <param name="lightValues"> The <see cref="LightValues"/> object. </param>
        /// <param name="type"> The type of the fetched property. </param>
        /// <returns> An object-boxed value. </returns>
        public static object GetBoxedPropertyValue(this TuningTarget tuningTarget, LightValues lightValues, out Type type)
        {
            type = tuningTarget.GetPropertyType();
            return tuningTarget.GetBoxedPropertyValue(lightValues);
        }

        /// <summary>
        /// Set the value of the property corresponding to this <see cref="TuningTarget"/>.
        /// </summary>
        /// <param name="tuningTarget"> This tuning target. </param>
        /// <param name="light"> The <see cref="Light"/> object. </param>
        /// <param name="value"> The object-boxed value. </param>
        /// <returns> True if successful. </returns>
        public static bool SetBoxedPropertyValue(this TuningTarget tuningTarget, Light light, object value)
        {
            switch (tuningTarget)
            {
                case TuningTarget.Range:
                    light.range = (float)value;
                    return true;

                case TuningTarget.Intensity:
                    light.intensity = (float)value;
                    return true;

                case TuningTarget.Color:
                    light.color = (Color)value;
                    return true;
            }
            return false;
        }
    }
}