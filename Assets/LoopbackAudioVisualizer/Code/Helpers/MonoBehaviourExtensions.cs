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

        /// <summary>
        /// Invokes the method 'action' with one argument after 'time' seconds.
        /// </summary>
        /// <param name="monoBehaviour"> This MonoBehaviour. </param>
        /// <param name="action"> The void-returning function to execute. </param>
        /// <param name="arg"> Argument. </param>
        /// <param name="time"> The delay, in seconds. </param>
        /// <typeparam name="T"> Type of argument. </typeparam>
        public static Coroutine Invoke<T>(this MonoBehaviour monoBehaviour, Action<T> action, T arg, float time)
        {
            return monoBehaviour.isActiveAndEnabled ? monoBehaviour.StartCoroutine(DelayedAction(action, arg, time)) : null;
        }

        /// <summary>
        /// Invokes the method 'action' with one argument after 'time' seconds.
        /// </summary>
        /// <param name="monoBehaviour"> This MonoBehaviour. </param>
        /// <param name="action"> The void-returning function to execute. </param>
        /// <param name="arg1"> Argument 1. </param>
        /// <param name="arg2"> Argument 2. </param>
        /// <param name="time"> The delay, in seconds. </param>
        /// <typeparam name="T1"> Type of argument 1. </typeparam>
        /// <typeparam name="T2"> Type of argument 2. </typeparam>
        public static Coroutine Invoke<T1, T2>(this MonoBehaviour monoBehaviour, Action<T1, T2> action, T1 arg1, T2 arg2, float time)
        {
            return monoBehaviour.isActiveAndEnabled ? monoBehaviour.StartCoroutine(DelayedAction(action, arg1, arg2, time)) : null;
        }

        private static IEnumerator DelayedAction(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

        private static IEnumerator DelayedAction<T>(Action<T> action, T arg, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke(arg);
        }

        private static IEnumerator DelayedAction<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke(arg1, arg2);
        }
    }
}