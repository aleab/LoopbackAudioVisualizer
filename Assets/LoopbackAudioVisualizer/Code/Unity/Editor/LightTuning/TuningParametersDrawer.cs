#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.LightTuning;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.LightTuning
{
    [CustomPropertyDrawer(typeof(TuningParameters))]
    public class TuningParametersDrawer : PropertyDrawer
    {
        private const string DEFAULT_NAME = "<Enter a name>";

        private SerializedProperty tuningName;
        private SerializedProperty tuningTarget;
        private SerializedProperty tuningType;
        private SerializedProperty internalTuningParameters;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = new Rect(position.x, position.y, position.width, EditorExtension.SingleLineHeight);
            EditorGUI.PropertyField(position, property, label, false);
            position = new Rect(position.x, position.y, position.width + EditorExtension.OneCharButtonWidth + 8.0f, EditorExtension.SingleLineHeight);

            if (property.isExpanded)
            {
                TuningParameters tuningParameters = Helpers.Helpers.GetActualObjectForSerializedProperty<TuningParameters>(property);
                if (tuningParameters == null)
                    return;

                LightSetTuningFactory.BuildInternalParameters(tuningParameters);

                Rect indentedRect = EditorGUI.IndentedRect(new Rect(position.x, position.y + 2.0f, position.width, EditorExtension.SingleLineHeight));
                Rect nameRect = new Rect(indentedRect.x, indentedRect.y + indentedRect.height, indentedRect.width, EditorExtension.SingleLineHeight);
                Rect targetRect = new Rect(indentedRect.x, nameRect.y + nameRect.height + 3.0f, indentedRect.width, EditorExtension.SingleLineHeight);
                Rect typeRect = new Rect(indentedRect.x, targetRect.y + targetRect.height, indentedRect.width, EditorExtension.SingleLineHeight);
                Rect parametersFoldoutRect = new Rect(indentedRect.x, typeRect.y + typeRect.height, indentedRect.width, EditorExtension.SingleLineHeight);

                string enteredName = EditorGUI.TextField(nameRect, GUIContent.none, string.IsNullOrWhiteSpace(this.tuningName.stringValue) ? DEFAULT_NAME : this.tuningName.stringValue, Styles.TextFieldBoldLabel);
                this.tuningName.stringValue = enteredName == DEFAULT_NAME ? null : enteredName;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(targetRect, this.tuningTarget, new GUIContent("Target"), false);
                EditorGUI.PropertyField(typeRect, this.tuningType, new GUIContent("Type"), false);
                if (EditorGUI.EndChangeCheck())
                    LightSetTuningFactory.BuildInternalParameters(tuningParameters);

                this.internalTuningParameters.isExpanded = EditorGUI.Foldout(parametersFoldoutRect, this.internalTuningParameters.isExpanded, new GUIContent("Tuning Parameters"));
                if (this.internalTuningParameters.isExpanded)
                {
                    Rect parametersRect = EditorGUI.IndentedRect(new Rect(parametersFoldoutRect.x, parametersFoldoutRect.y + parametersFoldoutRect.height, parametersFoldoutRect.width, parametersFoldoutRect.height));

                    InternalTuningParameters internalParameters = Helpers.Helpers.GetActualObjectForSerializedProperty<InternalTuningParameters>(this.internalTuningParameters);
                    InternalTuningParametersDrawer customDrawer = (InternalTuningParametersDrawer)Extensions.Helpers.GetPropertyDrawerForType(internalParameters.GetType());
                    customDrawer?.GetPropertyHeight(this.internalTuningParameters, GUIContent.none);
                    customDrawer?.OnGUI(parametersRect, this.internalTuningParameters, GUIContent.none);

                    internalParameters.DeserializeParameters();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.tuningName = property.FindPropertyRelative(nameof(this.tuningName));
            this.tuningTarget = property.FindPropertyRelative(nameof(this.tuningTarget));
            this.tuningType = property.FindPropertyRelative(nameof(this.tuningType));
            this.internalTuningParameters = property.FindPropertyRelative(nameof(this.internalTuningParameters));

            float height = EditorExtension.SingleLineHeight;
            if (property.isExpanded)
            {
                height += 2.0f + EditorExtension.SingleLineHeight + 3.0f + // TuningName
                          EditorExtension.SingleLineHeight +               // TuningTarget
                          EditorExtension.SingleLineHeight;                // TuningType

                if (this.internalTuningParameters.isExpanded)
                {
                    TuningParameters tuningParameters = Helpers.Helpers.GetActualObjectForSerializedProperty<TuningParameters>(property);
                    InternalTuningParametersDrawer customDrawer = (InternalTuningParametersDrawer)Extensions.Helpers.GetPropertyDrawerForType(tuningParameters?.InternalTuningParameters?.GetType());
                    height += customDrawer?.GetPropertyHeight(this.internalTuningParameters, GUIContent.none) ?? EditorGUI.GetPropertyHeight(this.internalTuningParameters, true);
                }
            }

            return height;
        }
    }
}

#endif