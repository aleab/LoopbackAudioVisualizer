using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aleab.LoopbackAudioVisualizer
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Scenes
    {
        public const string SCENES_PATH = "Assets/LoopbackAudioVisualizer";

        public static UnityScene Common { get; } = new UnityScene("00_Common", $"{SCENES_PATH}/00_Common.unity");
        public static UnityScene AudioVisualizer01 { get; } = new UnityScene("AudioVisualizer01", $"{SCENES_PATH}/AudioVisualizer01.unity");

#if UNITY_EDITOR
        public static UnityScene AudioVisualizer01_EditorOnly { get; } = new UnityScene("AudioVisualizer01_EditorOnly", $"{SCENES_PATH}/AudioVisualizer01_EditorOnly.unity");

#endif

#if DEBUG
        public static UnityScene DevScene { get; } = new UnityScene("DevScene", $"{SCENES_PATH}/DevScene.unity");
#endif
    }

    public class UnityScene
    {
        public string Name { get; }
        public string Path { get; }

        internal UnityScene(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public Scene Load(LoadSceneMode loadMode = LoadSceneMode.Single)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                string projectDir = Application.dataPath.Replace("Assets", string.Empty);
                FileInfo sceneFileInfo = new FileInfo(System.IO.Path.Combine(projectDir, this.Path));

                if (loadMode == LoadSceneMode.Single)
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                return sceneFileInfo.Exists ? EditorSceneManager.OpenScene(this.Path, (OpenSceneMode)loadMode) : new Scene();
            }
#endif
            SceneManager.LoadScene(this.Name, loadMode);
            return SceneManager.GetSceneByName(this.Name);
        }

        public bool IsLoaded()
        {
            Scene scene = SceneManager.GetSceneByName(this.Name);
            return scene.IsValid() && scene.isLoaded && scene.name == this.Name;
        }
    }
}