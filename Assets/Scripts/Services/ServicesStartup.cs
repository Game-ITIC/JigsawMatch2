using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Initializers;
using Itic.Scopes;
using Services;
using UnityEngine;
using VContainer.Unity;

namespace Itic.Services
{
    public class ServicesStartup : IAsyncStartable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly AdsInitializer _adsInitializer;

        public ServicesStartup(
            SceneLoader sceneLoader,
            AdsInitializer adsInitializer
        )
        {
            _sceneLoader = sceneLoader;
            _adsInitializer = adsInitializer;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            try
            {
                await _adsInitializer.Warmup();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ServicesStartup] AdsInitializer warmup warning: {e.Message}");
            }

            await UniTask.SwitchToMainThread();
            await _sceneLoader.LoadMenuAsync();
        }
    }
}
