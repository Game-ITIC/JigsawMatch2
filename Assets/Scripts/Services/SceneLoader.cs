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

        private const float LoadProgressStart = 0.35f;
        private const float LoadProgressEnd = 0.98f;

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
            await LoadSceneAsync("MainMenu");
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
            if (_sceneModel.LastCountryConfig == null)
            {
                await LoadAsiaRegionAsync();
            }
            else
            {
                await LoadRegionScene(_sceneModel.LastCountryConfig.countryId);
            }
        }

        private async UniTask LoadSceneAsync(int index)
        {
            await EnsureLoadingScreenVisibleAsync();

            var unloadSceneAsyncTask = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            if (unloadSceneAsyncTask != null)
            {
                while (!unloadSceneAsyncTask.isDone)
                {
                    _screenService.SetLoadingProgress(MapUnloadProgress(unloadSceneAsyncTask.progress));
                    await UniTask.Yield();
                }
            }

            var sceneLoadingOperation = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);

            while (sceneLoadingOperation is { isDone: false })
            {
                _screenService.SetLoadingProgress(MapLoadProgress(sceneLoadingOperation.progress));
                await UniTask.Yield();
            }

            var scene = SceneManager.GetSceneByBuildIndex(index);

            SceneManager.SetActiveScene(scene);

            var rootGameObjects = scene.GetRootGameObjects();

            foreach (var scope in rootGameObjects)
            {
                if (!scope.TryGetComponent(out ScopeInstaller installer))
                {
                    continue;
                }

                _screenService.SetLoadingProgress(0.92f);
                await installer.InstallScopeAsync();
                break;
            }

            _screenService.SetLoadingProgress(LoadProgressEnd);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            OnSceneLoaded?.Invoke();
            await _screenService.HideLoadingScreenAsync();
        }

        private async UniTask LoadSceneAsync(string sceneName)
        {
            await EnsureLoadingScreenVisibleAsync();

            var unloadSceneAsyncTask = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            if (unloadSceneAsyncTask != null)
            {
                while (!unloadSceneAsyncTask.isDone)
                {
                    _screenService.SetLoadingProgress(MapUnloadProgress(unloadSceneAsyncTask.progress));
                    await UniTask.Yield();
                }
            }

            var sceneLoadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (sceneLoadingOperation is { isDone: false })
            {
                _screenService.SetLoadingProgress(MapLoadProgress(sceneLoadingOperation.progress));
                await UniTask.Yield();
            }

            var scene = SceneManager.GetSceneByName(sceneName);

            SceneManager.SetActiveScene(scene);

            var rootGameObjects = scene.GetRootGameObjects();

            foreach (var scope in rootGameObjects)
            {
                if (!scope.TryGetComponent(out ScopeInstaller installer))
                {
                    continue;
                }

                _screenService.SetLoadingProgress(0.92f);
                await installer.InstallScopeAsync();
                break;
            }

            _screenService.SetLoadingProgress(LoadProgressEnd);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            OnSceneLoaded?.Invoke();
            await _screenService.HideLoadingScreenAsync();
        }

        async UniTask EnsureLoadingScreenVisibleAsync()
        {
            if (!_screenService.IsLoadingScreenVisible)
            {
                await _screenService.ShowLoadingScreenAsync();
                _screenService.SetLoadingProgress(LoadProgressStart);
            }
        }

        static float MapUnloadProgress(float progress)
        {
            return LoadProgressStart + Mathf.Clamp01(progress) * 0.08f;
        }

        static float MapLoadProgress(float progress)
        {
            return LoadProgressStart + 0.08f + Mathf.Clamp01(progress) * (LoadProgressEnd - LoadProgressStart - 0.08f);
        }
    }
}
