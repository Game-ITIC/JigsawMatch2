using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Itic.Services
{
    public class Startup : IAsyncStartable
    {
        public async UniTask StartAsync(CancellationToken token)
        {
            Application.targetFrameRate = 120;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //var splashTask = _splashScreenView.PlaySplashAsync(token);

            await InitializePlugins();

            await SceneManager.LoadSceneAsync("Services", LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: token);

            //await UniTask.WhenAll(splashTask, servicesTask);

            var servicesScene = SceneManager.GetSceneByName("Services");

            foreach (var rootGameObject in servicesScene.GetRootGameObjects())
            {
                if (rootGameObject.TryGetComponent(out LifetimeScope scope))
                {
                    scope.Build();
                    break;
                }
            }
        }

        private async UniTask InitializePlugins()
        {
            DOTween.Init();
            DOTween.SetTweensCapacity(84, 42);
            await UniTask.CompletedTask;
        }
        
        
    }
}