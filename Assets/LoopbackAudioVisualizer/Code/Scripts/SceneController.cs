using Aleab.LoopbackAudioVisualizer.Scripts.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aleab.LoopbackAudioVisualizer.Scripts
{
    public class SceneController : MonoBehaviour
    {
        #region Singleton

        private static SceneController _instance;

        public static SceneController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject($"[C] {nameof(SceneController)}");
                    _instance = go.AddComponent<SceneController>();

                    Debug.LogWarning("SceneController object has been created automatically");
                }
                return _instance;
            }
        }

        #endregion Singleton

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);

            this.SubscribeToSceneManagerEvents();
        }

        private void SubscribeToSceneManagerEvents()
        {
            this.UnsubscribeFromSceneManagerEvents();
            SceneManager.activeSceneChanged += this.SceneManager_activeSceneChanged;
            SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += this.SceneManager_sceneUnloaded;
        }

        private void UnsubscribeFromSceneManagerEvents()
        {
            SceneManager.activeSceneChanged -= this.SceneManager_activeSceneChanged;
            SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded -= this.SceneManager_sceneUnloaded;
        }

        private void OnEnable()
        {
            this.SubscribeToSceneManagerEvents();
        }

        private void OnDisable()
        {
            this.UnsubscribeFromSceneManagerEvents();
        }

        private void OnDestroy()
        {
            this.UnsubscribeFromSceneManagerEvents();
        }

        private void SceneManager_activeSceneChanged(Scene previous, Scene current)
        {
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Remove old menu items
            UIController.SettingsMenu.TabMenu.RemoveNonDefaultItems();

            // Find TabMenuItem components in the new scene
            List<TabMenuItem> menuItems = new List<TabMenuItem>();
            scene.GetRootGameObjects().ToList().ForEach(go => menuItems.AddRange(go.GetComponentsInChildren<TabMenuItem>()));
            UIController.SettingsMenu.TabMenu.AddItems(menuItems);
        }

        private void SceneManager_sceneUnloaded(Scene scene)
        {
        }
    }
}