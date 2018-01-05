#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.LightTuning.TuningFunctions;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.LightTuning.TuningFunctions
{
    [CustomPropertyDrawer(typeof(OnOffThresholdTuningFloatParameters), true)]
    public sealed class OnOffThresholdTuningFloatParametersDrawer : InternalTuningParametersDrawer
    {
        private const string E_TOOLTIP = "Defines the smoothness of the transition from 0 to 1.\nThe higher this value is, the closer the transition function will be to a step function.";

        #region Accessors to get/set the values from/to the parameters collection

        private float XMin
        {
            get { return this.GetParameterValue<float>(nameof(this.XMin)); }
            set { this.SetParameterValue(nameof(this.XMin), value); }
        }

        private float XMax
        {
            get { return this.GetParameterValue<float>(nameof(this.XMax)); }
            set { this.SetParameterValue(nameof(this.XMax), value); }
        }

        private float Threshold
        {
            get { return this.GetParameterValue<float>(nameof(this.Threshold)); }
            set { this.SetParameterValue(nameof(this.Threshold), value); }
        }

        private float E
        {
            get { return this.GetParameterValue<float>(nameof(this.E)); }
            set { this.SetParameterValue(nameof(this.E), value); }
        }

        private float YMin
        {
            get { return this.GetParameterValue<float>(nameof(this.YMin)); }
            set { this.SetParameterValue(nameof(this.YMin), value); }
        }

        private float YMax
        {
            get { return this.GetParameterValue<float>(nameof(this.YMax)); }
            set { this.SetParameterValue(nameof(this.YMax), value); }
        }

        #endregion Accessors to get/set the values from/to the parameters collection

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            float baseHeigh = base.GetPropertyHeight(property, label);
            Rect currentPosition = new Rect(position.x, position.y + baseHeigh + 3.0f, position.width, position.height);
            Rect xMinRect = new Rect(currentPosition.x, currentPosition.y + 3.0f, currentPosition.width, EditorExtension.SingleLineHeight);
            Rect xMaxRect = new Rect(currentPosition.x, xMinRect.y + xMinRect.height, currentPosition.width, EditorExtension.SingleLineHeight);
            Rect thresholdRect = new Rect(currentPosition.x, xMaxRect.y + xMaxRect.height + 3.0f, currentPosition.width, EditorExtension.SingleLineHeight);
            Rect eRect = new Rect(currentPosition.x, thresholdRect.y + thresholdRect.height, currentPosition.width, EditorExtension.SingleLineHeight);
            Rect yMinRect = new Rect(currentPosition.x, eRect.y + +eRect.height + 3.0f, currentPosition.width, EditorExtension.SingleLineHeight);
            Rect yMaxRect = new Rect(currentPosition.x, yMinRect.y + yMinRect.height, currentPosition.width, EditorExtension.SingleLineHeight);

            float xMin = this.XMin;
            float xMax = this.XMax;
            float e = this.E;
            float yMin = this.YMin;
            float yMax = this.YMax;

            EditorGUI.BeginChangeCheck();
            xMin = EditorGUI.Slider(xMinRect, new GUIContent("Min. X"), xMin, OnOffThresholdTuningFloatParameters.XMIN_MIN, xMax);
            if (EditorGUI.EndChangeCheck())
                this.XMin = xMin;

            EditorGUI.BeginChangeCheck();
            xMax = EditorGUI.Slider(xMaxRect, new GUIContent("Max. X"), xMax, xMin, OnOffThresholdTuningFloatParameters.XMAX_MAX);
            if (EditorGUI.EndChangeCheck())
                this.XMax = xMax;

            float threshold = this.Threshold;
            EditorGUI.BeginChangeCheck();
            threshold = EditorGUI.Slider(thresholdRect, new GUIContent("Threshold"), threshold, xMin, xMax);
            if (EditorGUI.EndChangeCheck())
                this.Threshold = threshold;

            EditorGUI.BeginChangeCheck();
            e = EditorGUI.Slider(eRect, new GUIContent("E", E_TOOLTIP), e, OnOffThresholdTuningFloatParameters.E_MIN, OnOffThresholdTuningFloatParameters.E_MAX);
            if (EditorGUI.EndChangeCheck())
                this.E = e;

            EditorGUI.BeginChangeCheck();
            yMin = EditorGUI.Slider(yMinRect, new GUIContent("Min. Y"), yMin, OnOffThresholdTuningFloatParameters.YMIN_MIN, yMax);
            if (EditorGUI.EndChangeCheck())
                this.YMin = yMin;

            EditorGUI.BeginChangeCheck();
            yMax = EditorGUI.Slider(yMaxRect, new GUIContent("Max. Y"), yMax, yMin, OnOffThresholdTuningFloatParameters.YMAX_MAX);
            if (EditorGUI.EndChangeCheck())
                this.YMax = yMax;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label) + 3.0f +
                           EditorExtension.SingleLineHeight + 3.0f +
                           EditorExtension.SingleLineHeight +
                           EditorExtension.SingleLineHeight + 3.0f +
                           EditorExtension.SingleLineHeight + 3.0f +
                           EditorExtension.SingleLineHeight + 3.0f +
                           EditorExtension.SingleLineHeight;
            return height;
        }
    }
}

#endif