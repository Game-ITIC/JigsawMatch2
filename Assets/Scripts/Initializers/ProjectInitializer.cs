using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Itic.Scopes;
using JetBrains.Annotations;
using Services;
using VContainer.Unity;
using Views;

namespace Initializers
{
    [UsedImplicitly]
    public class ProjectInitializer : IAsyncStartable
    {
        private readonly IronSourceInitializer _ironSourceInitializer;
        private readonly IronSourceManager _ironSourceManager;
        private readonly InternetChecker _internetChecker;
        private readonly SceneLoader _sceneLoader;
        private readonly InternetState _internetState;
        private readonly LoadingScreenView _loadingScreenView;

        public ProjectInitializer(
            IronSourceInitializer ironSourceInitializer,
            IronSourceManager ironSourceManager,
            InternetChecker internetChecker,
            SceneLoader sceneLoader,
            InternetState internetState,
            LoadingScreenView loadingScreenView
        )
        {
            _ironSourceInitializer = ironSourceInitializer;
            _ironSourceManager = ironSourceManager;
            _internetChecker = internetChecker;
            _sceneLoader = sceneLoader;
            _internetState = internetState;
            _loadingScreenView = loadingScreenView;
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            
            await _sceneLoader.LoadRegionAsync();
            return;

            var hasInternetAccess = await _internetChecker.HasInternetAccess();
            _internetState.HasInternet = hasInternetAccess;
            _loadingScreenView.Show();
            if (hasInternetAccess)
            {
                var isReady = await _ironSourceInitializer.WaitForIronSourceInit();
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

                if (isReady && _internetState.HasRemoveAds)
                {
                    _ironSourceManager.InitializeLevelPlay();
                }
            }

            await _sceneLoader.LoadGameAsync();
            _loadingScreenView.Hide();
        }
    }
}