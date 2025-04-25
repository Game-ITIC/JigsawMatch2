using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Itic.Scopes;
using Services;
using VContainer.Unity;

namespace Itic.Services
{
    public class ServicesStartup : IAsyncStartable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly InternetState _internetState;
        private readonly InternetChecker _internetChecker;

        public ServicesStartup(SceneLoader sceneLoader, 
            InternetState internetState,
            InternetChecker internetChecker
            )
        {
            _sceneLoader = sceneLoader;
            _internetState = internetState;
            _internetChecker = internetChecker;
        }
        
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            
            var hasInternetAccess = await _internetChecker.HasInternetAccess();
            _internetState.HasInternet = hasInternetAccess;
            
            if (hasInternetAccess)
            {
                // var isReady = await _ironSourceInitializer.WaitForIronSourceInit();
                var completionSource = new UniTaskCompletionSource();
            
                API.Initialize((status, message) =>
                {
                    completionSource.TrySetResult();
                    if (status == IAPOperationStatus.Success)
                    {
                        _internetState.HasRemoveAds = API.IsActive(ShopProductNames.RemoveAds);
                    }
                });
            
                await completionSource.Task;
            
                // if (isReady && _internetState.HasRemoveAds)
                // {
                //     _ironSourceManager.InitializeLevelPlay();
                // }
            }
            
            await _sceneLoader.LoadRegionAsync();
        }
    }
}