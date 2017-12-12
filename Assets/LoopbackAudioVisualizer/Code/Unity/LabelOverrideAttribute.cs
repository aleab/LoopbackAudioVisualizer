using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelOverrideAttribute : PropertyAttribute
    {
        public readonly string label;

        public LabelOverrideAttribute(string label)
        {
            this.label = label;
        }
    }
}