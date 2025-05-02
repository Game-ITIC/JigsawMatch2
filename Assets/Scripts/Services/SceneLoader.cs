using System;
using Cysharp.Threading.Tasks;
using Data;
using Itic.Services;
using Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Itic.Scopes
{
    public class SceneLoader
    {
        public event Action OnSceneLoaded = delegate { };

        private readonly ScreenService _screenService;
        private readonly SceneModel _sceneModel;

        public SceneLoader(
            ScreenService screenService,
            SceneModel sceneModel
        )
        {
            _screenService = screenService;
            _sceneModel = sceneModel;
        }

        public async UniTask LoadMenuAsync()
        {
            await LoadSceneAsync(2);
        }

        public async UniTask LoadGameAsync()
        {
            await LoadSceneAsync(3);
        }

        public async UniTask LoadMapAsync()
        {
            await LoadSceneAsync(5);
        }

        public async UniTask LoadAsiaRegionAsync()
        {
            await LoadSceneAsync(6);
        }

        public async UniTask LoadEgyptRegionAsync()
        {
            await LoadSceneAsync(7);
        }

        public async UniTask LoadRegionScene(CountryEnum country)
        {
            switch (country)
            {
                case CountryEnum.Asia: await LoadAsiaRegionAsync(); break;
                case CountryEnum.Egypt: await LoadEgyptRegionAsync(); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(country), country, null);
            }
        }
        
        public async UniTask LoadLastSceneAsync()
        {
            if (_sceneModel.LastCountryConfig == null) return;

            await LoadRegionScene(_sceneModel.LastCountryConfig.countryId);
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