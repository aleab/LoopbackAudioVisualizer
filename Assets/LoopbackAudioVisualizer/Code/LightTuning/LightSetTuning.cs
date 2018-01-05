using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public abstract class LightSetTuning<TIn, TTarget> : ILightSetTuning
    {
        private readonly Object ipComponent;
        private readonly MethodInfo ipMethod;

        protected LightSetTuning(InternalTuningParameters parameters)
        {
            if (typeof(TIn).FullName != parameters.IPReturnType)
                throw new ArgumentException("Incorrect input type.");

            this.ipComponent = parameters.IPComponent;
            if (this.ipComponent != null)
                this.ipMethod = this.GetMethodInfo(this.ipComponent.GetType(), parameters.IPName);
        }

        private MethodInfo GetMethodInfo(Type type, string methodName)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                       .Single(m => m.Name == methodName && LightSetTuning.CheckMethodForInputProvider(m, typeof(TIn)));
        }

        /// <inheritdoc />
        public void Tune(Light light, TuningTarget tuningTarget, object defaultValue)
        {
            if (!(defaultValue is TTarget))
                return;

            this.Tune(light, tuningTarget, (TTarget)defaultValue);
        }

        /// <inheritdoc />
        public void Tune(Light light, TuningTarget tuningTarget)
        {
            if (this.ipComponent == null || this.ipMethod == null)
                return;

            TIn x = (TIn)this.ipMethod.Invoke(this.ipComponent, new object[] { light });
            TTarget y = this.ProcessInput(x);
            tuningTarget.SetBoxedPropertyValue(light, y);
        }

        private void Tune(Light light, TuningTarget tuningTarget, TTarget defaultValue)
        {
            if (this.ipComponent == null || this.ipMethod == null)
                return;

            TIn x = (TIn)this.ipMethod.Invoke(this.ipComponent, new object[] { light });
            TTarget y = this.ProcessInput(x, defaultValue);
            tuningTarget.SetBoxedPropertyValue(light, y);
        }

        protected abstract TTarget ProcessInput(TIn x);

        protected abstract TTarget ProcessInput(TIn x, TTarget defaultValue);
    }

    public static class LightSetTuning
    {
        /// <summary>
        /// Checks if the specified method is a compatible input provider for <see cref="LightSetTuning{TIn,TTarget}"/>.
        /// </summary>
        /// <param name="method"> The method to check. </param>
        /// <param name="inType"> The input type of <see cref="LightSetTuning{TIn,TTarget}"/>; the expected return type of the method. </param>
        /// <returns> True or false. </returns>
        public static bool CheckMethodForInputProvider(MethodInfo method, Type inType)
        {
            if (method.ReturnType != inType)
                return false;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;

            return parameters[0].ParameterType == typeof(Light);
        }
    }

    public interface ILightSetTuning
    {
        void Tune(Light light, TuningTarget tuningTarget);

        void Tune(Light light, TuningTarget tuningTarget, object defaultValue);
    }
}