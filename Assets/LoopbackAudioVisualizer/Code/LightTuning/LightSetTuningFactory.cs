using Aleab.LoopbackAudioVisualizer.LightTuning.TuningFunctions;
using System;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.LightTuning
{
    public class LightSetTuningFactory
    {
        public static ILightSetTuning CreateLightSetTuning(TuningParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            BuildInternalParameters(parameters);

            ILightSetTuning lightSetTuning = null;
            switch (parameters.TuningType)
            {
                case TuningType.OnOffThreshold:
                    switch (parameters.TuningTarget)
                    {
                        case TuningTarget.Range:
                        case TuningTarget.Intensity:
                            lightSetTuning = new OnOffThresholdTuningFloat((OnOffThresholdTuningFloatParameters)parameters.InternalTuningParameters);
                            break;

                        case TuningTarget.Color:
                            break;
                    }
                    break;
            }
            return lightSetTuning;
        }

        public static void BuildInternalParameters(TuningParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            parameters.StopListenForInternalTuningParametersChanges();

            var internalParameters = parameters.InternalTuningParameters;
            switch (parameters.TuningType)
            {
                case TuningType.OnOffThreshold:
                    switch (parameters.TuningTarget)
                    {
                        case TuningTarget.Range:
                        case TuningTarget.Intensity:
                            if (parameters.InternalTuningParameters is OnOffThresholdTuningFloatParameters)
                            {
                                parameters.InternalTuningParameters = (OnOffThresholdTuningFloatParameters)internalParameters;
                                parameters.InternalTuningParameters.ReplaceParameters(internalParameters);
                            }
                            else
                                parameters.InternalTuningParameters = new OnOffThresholdTuningFloatParameters(internalParameters);
                            parameters.InternalTuningParameters.IPReturnType = typeof(float).FullName;
                            break;

                        case TuningTarget.Color:
                            parameters.InternalTuningParameters = new InternalTuningParameters(internalParameters) { IPReturnType = typeof(Color).FullName };
                            break;
                    }
                    break;
            }

            // Deserialize and re-serialize:
            // Deserialize to copy the serialized values to the actual instance fields;
            // re-serialize to clean the list of serialized parameters of possible obsolete entries.
            parameters.InternalTuningParameters?.DeserializeParameters();
            parameters.InternalTuningParameters?.SerializeParameters();

            parameters.ListenForInternalTuningParametersChanges();
        }
    }
}