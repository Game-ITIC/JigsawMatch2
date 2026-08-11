using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Initializers;
using Itic.Scopes;
using UnityEngine;
using VContainer.Unity;
using Debug = UnityEngine.Debug;

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
            var sw = Stopwatch.StartNew();
            Debug.Log("[ServicesStartup] === STEP 2/3: Starting ServicesStartup ===");

            try
            {
                Debug.Log("[ServicesStartup] Starting AdsInitializer.Warmup()...");
                var adsSw = Stopwatch.StartNew();
                await _adsInitializer.Warmup();
                Debug.Log($"[ServicesStartup] AdsInitializer.Warmup() finished in {adsSw.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServicesStartup] AdsInitializer warmup failed with error: {e}");
            }

            await UniTask.SwitchToMainThread();

            Debug.Log($"[ServicesStartup] Requesting SceneLoader.LoadMenuAsync() (Total elapsed: {sw.ElapsedMilliseconds} ms)...");
            await _sceneLoader.LoadMenuAsync();
            Debug.Log($"[ServicesStartup] === STEP 2/3 Complete: ServicesStartup finished in {sw.ElapsedMilliseconds} ms ===");
        }
    }
}
