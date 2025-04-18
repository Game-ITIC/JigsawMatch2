using System.Collections.Generic;
using Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Views;

namespace Services
{
    public class SceneLoader : ISceneLoader
    {
        private readonly SceneLoadingConfig _config;
        private readonly LoadingScreenView screenView;

        public SceneLoader(SceneLoadingConfig config, LoadingScreenView screenView)
        {
            _config = config;
            this.screenView = screenView;
        }

        public async UniTask LoadGameplayScene()
        {
            await LoadScene(_config.GameplaySceneName);
        }

        public async UniTask LoadScene(string sceneName)
        {
            try
            {
                screenView.Show();
                await UnloadCurrentScenes();
                await LoadSceneAdditive(sceneName);
                SetActiveScene(sceneName);
                screenView.Hide();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Scene load failed: {e}");
                throw;
            }
        }

        public async UniTask UnloadCurrentScenes()
        {
            var unloadTasks = new List<UniTask>();
        
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (ShouldUnloadScene(scene))
                {
                    unloadTasks.Add(UnloadSceneAsync(scene));
                }
            }

            await UniTask.WhenAll(unloadTasks);
        }

        private async UniTask LoadSceneAdditive(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await operation.ToUniTask();
        }

        private async UniTask UnloadSceneAsync(Scene scene)
        {
            var operation = SceneManager.UnloadSceneAsync(scene);
            await operation.ToUniTask();
        }

        private void SetActiveScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
            }
        }

        private bool ShouldUnloadScene(Scene scene)
        {
            return !_config.PersistentScenes.Contains(scene.name);
        }
    }
}