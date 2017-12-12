using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Helpers
{
    internal static class MonoBehaviourExtensions
    {
        public static void RequireField(this MonoBehaviour monoBeheviour, [NotNull] string fieldName, object fieldValue)
        {
            if (fieldValue == null)
            {
                string message = $"Field \"{fieldName}\" in {monoBeheviour.GetType().Name} ({monoBeheviour.gameObject.name}) cannot be null!";
                Debug.LogError(message);
                throw new ArgumentNullException(fieldName, message);
            }
        }

        /// <summary>
        /// Invokes the method 'action' after 'time' seconds.
        /// </summary>
        /// <param name="monoBehaviour"> This MonoBehaviour. </param>
        /// <param name="action"> The void-returning function to execute. </param>
        /// <param name="time"> The delay, in seconds. </param>
        public static Coroutine Invoke(this MonoBehaviour monoBehaviour, Action action, float time)
        {
            return monoBehaviour.isActiveAndEnabled ? monoBehaviour.StartCoroutine(DelayedAction(action, time)) : null;
        }

        private static IEnumerator DelayedAction(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }
}