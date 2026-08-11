using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Interfaces;
using Itic.Scopes;
using Itic.Services;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using VContainer.Unity;
using Views;

namespace Initializers
{
    [UsedImplicitly]
    public class AdsInitializer : IPreload
    {
        private readonly IronSourceInitializer _ironSourceInitializer;
        private readonly InternetChecker _internetChecker;
        private readonly InternetState _internetState;
        private readonly ScreenService _screenService;

        private const int INTERNET_CHECK_TIMEOUT = 10000;
        private const int IRONSOURCE_INIT_TIMEOUT = 15000;
        private const int IAP_INIT_TIMEOUT = 10000;

        public AdsInitializer(
            IronSourceInitializer ironSourceInitializer,
            InternetChecker internetChecker,
            SceneLoader sceneLoader,
            InternetState internetState,
            ScreenService screenService
        )
        {
            _ironSourceInitializer = ironSourceInitializer;
            _internetChecker = internetChecker;
            _internetState = internetState;
            _screenService = screenService;
        }

        public async UniTask Warmup()
        {
            await _screenService.ShowLoadingScreenAsync();
            _screenService.SetLoadingProgress(0f);

            try
            {
                _screenService.SetLoadingProgress(0.08f);
                var hasInternetAccess = await CheckInternetWithTimeout();
                await UniTask.SwitchToMainThread();
                _internetState.HasInternet = hasInternetAccess;
                _screenService.SetLoadingProgress(0.18f);

                if (hasInternetAccess)
                {
                    await WaitForIronSourceWithTimeout();
                    await UniTask.SwitchToMainThread();
                    _screenService.SetLoadingProgress(0.26f);

                    await InitializeIAPWithTimeout();
                    await UniTask.SwitchToMainThread();
                    _screenService.SetLoadingProgress(0.32f);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AdsInitializer] Warmup non-critical exception: {ex.Message}");
                _internetState.HasInternet = false;
                _internetState.HasRemoveAds = false;
            }

            await UniTask.SwitchToMainThread();
            _screenService.SetLoadingProgress(0.35f);
        }

        private async UniTask<bool> CheckInternetWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(INTERNET_CHECK_TIMEOUT))
                {
                    var result = await _internetChecker.HasInternetAccess().AttachExternalCancellation(cts.Token);
                    await UniTask.SwitchToMainThread();
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                await UniTask.SwitchToMainThread();
                return false;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                return false;
            }
        }

        private async UniTask<bool> WaitForIronSourceWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(IRONSOURCE_INIT_TIMEOUT))
                {
                    var result = await _ironSourceInitializer.WaitForIronSourceInit().AttachExternalCancellation(cts.Token);
                    await UniTask.SwitchToMainThread();
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                await UniTask.SwitchToMainThread();
                return false;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                return false;
            }
        }

        private async UniTask InitializeIAPWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(IAP_INIT_TIMEOUT))
                {
                    var completionSource = new UniTaskCompletionSource();

                    cts.Token.Register(() =>
                    {
                        if (!completionSource.Task.Status.IsCompleted())
                        {
                            completionSource.TrySetCanceled();
                        }
                    });

                    API.Initialize((status, message) =>
                    {
                        try
                        {
                            if (status == IAPOperationStatus.Success)
                            {
                                _internetState.HasRemoveAds = API.IsActive(ShopProductNames.RemoveAds);
                            }
                            else
                            {
                                _internetState.HasRemoveAds = false;
                            }
                        }
                        catch (Exception)
                        {
                            _internetState.HasRemoveAds = false;
                        }
                        finally
                        {
                            completionSource.TrySetResult();
                        }
                    });

                    await completionSource.Task;
                    await UniTask.SwitchToMainThread();
                }
            }
            catch (OperationCanceledException)
            {
                await UniTask.SwitchToMainThread();
                _internetState.HasRemoveAds = false;
            }
            catch (Exception)
            {
                await UniTask.SwitchToMainThread();
                _internetState.HasRemoveAds = false;
            }
        }
    }
}
