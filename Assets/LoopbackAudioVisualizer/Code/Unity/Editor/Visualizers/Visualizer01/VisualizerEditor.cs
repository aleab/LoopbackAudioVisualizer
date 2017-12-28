#if UNITY_EDITOR

using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Scripts.Visualizers.Visualizer01;
using Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aleab.LoopbackAudioVisualizer.Unity.UnityEditor.Visualizers.Visualizer01
{
    [CustomEditor(typeof(Visualizer))]
    public sealed class VisualizerEditor : Editor
    {
        private static readonly Dictionary<Visualizer, bool> circSpectrumFoldouts;
        private static readonly Dictionary<Visualizer, bool> ellipseSpectrumFoldouts;

        private Visualizer visualizer;

        private SerializedProperty spectrumVisualizer;

        private SerializedProperty circCubeTemplate;
        private SerializedProperty circCubesContainer;
        private SerializedProperty circCenter;
        private SerializedProperty circRadius;
        private SerializedProperty circCubeMaxHeight;

        private SerializedProperty ellipseCubeTemplate;
        private SerializedProperty ellipseCubesContainer;
        private SerializedProperty ellipseCenter;
        private SerializedProperty ellipseSemiMinorAxisLength;
        private SerializedProperty ellipseChordLength;
        private SerializedProperty ellipseArcDegree;
        private SerializedProperty ellipseCubeMaxHeight;

        static VisualizerEditor()
        {
            IEqualityComparer<Visualizer> comparer = new AnonymousComparer<Visualizer>((v1, v2) => v1.GetInstanceID() == v2.GetInstanceID());

            if (circSpectrumFoldouts != null)
                circSpectrumFoldouts.Clear();
            else
                circSpectrumFoldouts = new Dictionary<Visualizer, bool>(comparer);

            if (ellipseSpectrumFoldouts != null)
                ellipseSpectrumFoldouts.Clear();
            else
                ellipseSpectrumFoldouts = new Dictionary<Visualizer, bool>(comparer);
        }

        private void OnEnable()
        {
            this.visualizer = (Visualizer)this.target;

            this.spectrumVisualizer = this.serializedObject.FindProperty("spectrumVisualizer");

            this.circCubeTemplate = this.serializedObject.FindProperty("circCubeTemplate");
            this.circCubesContainer = this.serializedObject.FindProperty("circCubesContainer");
            this.circCenter = this.serializedObject.FindProperty("circCenter");
            this.circRadius = this.serializedObject.FindProperty("circRadius");
            this.circCubeMaxHeight = this.serializedObject.FindProperty("circCubeMaxHeight");

            this.ellipseCubeTemplate = this.serializedObject.FindProperty("ellipseCubeTemplate");
            this.ellipseCubesContainer = this.serializedObject.FindProperty("ellipseCubesContainer");
            this.ellipseCenter = this.serializedObject.FindProperty("ellipseCenter");
            this.ellipseSemiMinorAxisLength = this.serializedObject.FindProperty("ellipseSemiMinorAxisLength");
            this.ellipseChordLength = this.serializedObject.FindProperty("ellipseChordLength");
            this.ellipseArcDegree = this.serializedObject.FindProperty("ellipseArcDegree");
            this.ellipseCubeMaxHeight = this.serializedObject.FindProperty("ellipseCubeMaxHeight");

            if (!circSpectrumFoldouts.ContainsKey(this.visualizer))
                circSpectrumFoldouts.Add(this.visualizer, true);
            if (!ellipseSpectrumFoldouts.ContainsKey(this.visualizer))
                ellipseSpectrumFoldouts.Add(this.visualizer, true);
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorExtension.DrawPropertyFieldSafe(this.spectrumVisualizer, nameof(this.spectrumVisualizer), new GUIContent("Spectrum Visualizer"));


            // ===================[ Circumference Spectrum ]===================
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.HelpBoxForFoldout);
            circSpectrumFoldouts[this.visualizer] = EditorGUILayout.Foldout(circSpectrumFoldouts[this.visualizer], new GUIContent("[Spectrum] Circumference"), true, Styles.FoldoutWithBoldLabel);
            if (circSpectrumFoldouts[this.visualizer])
            {
                EditorGUI.indentLevel++;
                EditorExtension.DrawPropertyFieldSafe(this.circCubeTemplate, nameof(this.circCubeTemplate), new GUIContent("Cube Template"));
                EditorExtension.DrawPropertyFieldSafe(this.circCubesContainer, nameof(this.circCubesContainer), new GUIContent("Cubes Container"));

                EditorExtension.DrawHeader("Circumference");
                EditorExtension.DrawPropertyFieldSafe(this.circCenter, nameof(this.circCenter), new GUIContent("Center", "The center of the circumference the cube-frequencies are going to be placed upon."));
                EditorExtension.DrawPropertyFieldSafe(this.circRadius, nameof(this.circRadius), new GUIContent("Radius", "The radius of the circumference the cubes-frequencies are going to be placed upon."));

                EditorGUILayout.Space();
                EditorExtension.DrawPropertyFieldSafe(this.circCubeMaxHeight, nameof(this.circCubeMaxHeight), new GUIContent("Max. Height", "Maximum Y scale of each cube."));

                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(new GUIContent("Refresh Preview")))
                        this.visualizer.CreateEditorCircCubes();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();


            // ===================[ Ellipse Arc Spectrum ]===================
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(Styles.HelpBoxForFoldout);
            ellipseSpectrumFoldouts[this.visualizer] = EditorGUILayout.Foldout(ellipseSpectrumFoldouts[this.visualizer], new GUIContent("[Spectrum] Ellipse Arc"), true, Styles.FoldoutWithBoldLabel);
            if (ellipseSpectrumFoldouts[this.visualizer])
            {
                EditorGUI.indentLevel++;
                EditorExtension.DrawPropertyFieldSafe(this.ellipseCubeTemplate, nameof(this.ellipseCubeTemplate), new GUIContent("Cube Template"));
                EditorExtension.DrawPropertyFieldSafe(this.ellipseCubesContainer, nameof(this.ellipseCubesContainer), new GUIContent("Cubes Container"));

                EditorExtension.DrawHeader("Ellipse and Arc");
                EditorExtension.DrawPropertyFieldSafe(this.ellipseCenter, nameof(this.ellipseCenter), new GUIContent("Center", "The center of the ellipse."));
                EditorExtension.DrawPropertyFieldSafe(this.ellipseSemiMinorAxisLength, nameof(this.ellipseSemiMinorAxisLength), new GUIContent("Semi Minor Axis", "The length of half of the minor axis (b)."));

                GUILayout.Space(4.0f);
                EditorExtension.DrawPropertyFieldSafe(this.ellipseChordLength, nameof(this.ellipseChordLength), new GUIContent("Chord Length", "The length of the arc's chord."));
                EditorExtension.DrawPropertyFieldSafe(this.ellipseArcDegree, nameof(this.ellipseArcDegree), new GUIContent("Arc Angle", "The length of the arc in degrees."));

                EditorGUILayout.Space();
                EditorExtension.DrawPropertyFieldSafe(this.ellipseCubeMaxHeight, nameof(this.ellipseCubeMaxHeight), new GUIContent("Max. Height", "Maximum Y scale of each cube."));

                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(new GUIContent("Refresh Preview")))
                        this.visualizer.CreateEditorEllipseCubes();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif