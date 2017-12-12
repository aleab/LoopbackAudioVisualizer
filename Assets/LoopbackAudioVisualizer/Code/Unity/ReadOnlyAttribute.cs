using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}