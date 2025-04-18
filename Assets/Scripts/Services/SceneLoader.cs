using System;
using Cysharp.Threading.Tasks;
using Itic.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Itic.Scopes
{
    public class SceneLoader
    {
        public event Action OnSceneLoaded = delegate { };
        
        private readonly ScreenService _screenService;

        public SceneLoader(ScreenService screenService)
        {
            _screenService = screenService;
        }
        
        public async UniTask LoadMenuAsync()
        {
            await LoadSceneAsync(2);
        }
        
        public async UniTask LoadGameAsync()
        {
            await LoadSceneAsync(3);
        }
        
        public async UniTask LoadRegionAsync()
        {
            await LoadSceneAsync(6);
        }
        
        private async UniTask LoadSceneAsync(int index)
        {
            var unloadSceneAsyncTask = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            
            await _screenService.ShowLoadingScreenAsync();
            
            //TODO maybe it's better to use SceneManager.GetActiveScene().IsValid?
            if (unloadSceneAsyncTask != null)
            {
                await unloadSceneAsyncTask;
            }

            var sceneLoadingOperation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            
            await UniTask.WaitUntil(() => sceneLoadingOperation is { isDone: true });
            var scene = SceneManager.GetSceneByBuildIndex(index);

            SceneManager.SetActiveScene(scene);
            
            var rootGameObjects = scene.GetRootGameObjects();

            foreach (var scope in rootGameObjects)
            {
                Debug.Log(scope.name);
                if (!scope.TryGetComponent(out ScopeInstaller installer))
                {
                    continue;
                }

                await installer.InstallScopeAsync();
                break;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            OnSceneLoaded?.Invoke();
            await _screenService.HideLoadingScreenAsync();
        }
    }
}