using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class EditorCamera : MonoBehaviour
    {
        private Camera _camera;

        [SerializeField]
        private new bool enabled = true;

        private void Awake()
        {
            this._camera = this.GetComponent<Camera>();
        }

#if UNITY_EDITOR

        private void OnRenderObject()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (this.enabled)
                {
                    var editorCamera = Camera.current;
                    if (editorCamera != null)
                    {
                        this._camera.transform.position = editorCamera.transform.position;
                        this._camera.transform.rotation = editorCamera.transform.rotation;
                    }
                }
            }
        }

#endif
    }
}