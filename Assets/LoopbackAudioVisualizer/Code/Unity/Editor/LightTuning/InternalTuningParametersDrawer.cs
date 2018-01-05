#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.LightTuning;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.LightTuning
{
    [CustomPropertyDrawer(typeof(InternalTuningParameters), true)]
    public class InternalTuningParametersDrawer : PropertyDrawer
    {
        private InternalTuningParameters internalParameters;

        private Object targetObject;
        private SerializedProperty ipComponent;
        private SerializedProperty ipName;
        private SerializedProperty ipReturnType;
        protected SerializedProperty parametersNames;
        protected SerializedProperty parametersValues;

        protected void SetParameterValue<T>(string parameterName, T value) where T : IConvertible
        {
            this.internalParameters?.SetParameterValue(parameterName, value);

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MonoBehaviour targetObjectBehaviour = this.targetObject as MonoBehaviour;
                if (targetObjectBehaviour != null)
                    EditorSceneManager.MarkSceneDirty(targetObjectBehaviour.gameObject.scene);
            }
        }

        protected T GetParameterValue<T>(string parameterName) where T : IConvertible
        {
            T value = default(T);
            if (this.internalParameters != null)
                value = this.internalParameters.GetParameterValue<T>(parameterName);
            return value;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect labelRect = new Rect(position.x, position.y + 3.0f, position.width, EditorExtension.SingleLineHeight);
            Rect ipTotalRect = new Rect(position.x, labelRect.y + labelRect.height + 3.0f, position.width,
                                        EditorExtension.SingleLineHeight + (this.ipComponent.objectReferenceValue != null ? 3.0f + EditorExtension.SingleLineHeight : 0.0f));
            Rect ipComponentRect = new Rect(ipTotalRect.x, ipTotalRect.y, ipTotalRect.width - 8.0f, EditorExtension.SingleLineHeight);
            Rect ipMethodRect = new Rect(ipTotalRect.x, ipComponentRect.y + ipComponentRect.height + 3.0f, ipTotalRect.width - 8.0f, EditorExtension.SingleLineHeight);
            Rect boxRect = new Rect(position.x, position.y, position.width, 3.0f + labelRect.height + 3.0f + ipTotalRect.height + 3.0f);

            EditorGUI.BeginProperty(boxRect, GUIContent.none, this.ipComponent);
            GUI.Box(boxRect, GUIContent.none);
            EditorGUI.LabelField(labelRect, new GUIContent("Input Provider"));
            this.ipComponent.objectReferenceValue = EditorGUI.ObjectField(ipComponentRect, this.ipComponent.objectReferenceValue, typeof(MonoBehaviour), true);
            if (this.ipComponent.objectReferenceValue != null)
            {
                Type returnType = Type.GetType(this.ipReturnType.stringValue);

                Type componentType = this.ipComponent.objectReferenceValue.GetType();
                var compatibleMethods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => LightSetTuning.CheckMethodForInputProvider(m, returnType)).ToList();

                int index = compatibleMethods.FindIndex(m => m.Name == this.ipName.stringValue);
                if (index < 0)
                {
                    this.ipName.stringValue = null;
                    index = 0;
                }

                if (compatibleMethods.Count > 0)
                {
                    GUIContent[] options = new GUIContent[compatibleMethods.Count];
                    for (int i = 0; i < options.Length; ++i)
                        options[i] = new GUIContent(compatibleMethods[i].Name);

                    index = EditorGUI.Popup(ipMethodRect, index, options);
                    if (index >= 0)
                        this.ipName.stringValue = compatibleMethods[index].Name;
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.internalParameters = Extensions.Helpers.GetActualObjectForSerializedProperty<InternalTuningParameters>(property);

            this.targetObject = property.serializedObject.targetObject;
            this.ipComponent = property.FindPropertyRelative(nameof(this.ipComponent));
            this.ipName = property.FindPropertyRelative(nameof(this.ipName));
            this.ipReturnType = property.FindPropertyRelative(nameof(this.ipReturnType));
            this.parametersNames = property.FindPropertyRelative(nameof(this.parametersNames));
            this.parametersValues = property.FindPropertyRelative(nameof(this.parametersValues));

            return 3.0f + // Top box padding
                   EditorExtension.SingleLineHeight + 3.0f + // labelRect
                   EditorExtension.SingleLineHeight + (this.ipComponent.objectReferenceValue != null ? EditorExtension.SingleLineHeight + 3.0f : 0.0f) + // ipTotalRect
                   3.0f;  // Bottom box padding;
        }
    }
}

#endif