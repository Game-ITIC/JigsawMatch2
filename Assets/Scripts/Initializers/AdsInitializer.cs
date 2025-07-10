using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Interfaces;
using Itic.Scopes;
using JetBrains.Annotations;
using Services;
using VContainer.Unity;
using Views;

namespace Initializers
{
    [UsedImplicitly]
    public class AdsInitializer : IPreload
    {
        private readonly IronSourceInitializer _ironSourceInitializer;
        private readonly IronSourceManager _ironSourceManager;
        private readonly InternetChecker _internetChecker;
        private readonly SceneLoader _sceneLoader;
        private readonly InternetState _internetState;
        private readonly LoadingScreenView _loadingScreenView;

        // Таймауты в миллисекундах
        private const int INTERNET_CHECK_TIMEOUT = 10000; // 10 секунд
        private const int IRONSOURCE_INIT_TIMEOUT = 15000; // 15 секунд
        private const int IAP_INIT_TIMEOUT = 10000; // 10 секунд

        public AdsInitializer(
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

        public async UniTask Warmup()
        {   
            _loadingScreenView.Show();
            
            try
            {
                // Проверка интернета с таймаутом
                var hasInternetAccess = await CheckInternetWithTimeout();
                _internetState.HasInternet = hasInternetAccess;

                if (hasInternetAccess)
                {
                    // IronSource инициализация с таймаутом
                    var isReady = await WaitForIronSourceWithTimeout();
                    
                    // IAP инициализация с таймаутом
                    await InitializeIAPWithTimeout();

                    if (isReady && _internetState.HasRemoveAds)
                    {
                        _ironSourceManager.InitializeLevelPlay();
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и продолжаем работу
                UnityEngine.Debug.LogError($"AdsInitializer error: {ex.Message}");
                // Устанавливаем значения по умолчанию
                _internetState.HasInternet = false;
                _internetState.HasRemoveAds = false;
            }

            // await _sceneLoader.LoadGameAsync();
            _loadingScreenView.Hide();
        }

        private async UniTask<bool> CheckInternetWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(INTERNET_CHECK_TIMEOUT))
                {
                    return await _internetChecker.HasInternetAccess().AttachExternalCancellation(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.LogWarning("Internet check timeout");
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Internet check error: {ex.Message}");
                return false;
            }
        }

        private async UniTask<bool> WaitForIronSourceWithTimeout()
        {
            try
            {
                using (var cts = new CancellationTokenSource(IRONSOURCE_INIT_TIMEOUT))
                {
                    return await _ironSourceInitializer.WaitForIronSourceInit().AttachExternalCancellation(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.LogWarning("IronSource initialization timeout");
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"IronSource initialization error: {ex.Message}");
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
                    
                    // Регистрируем отмену таймаута
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
                                UnityEngine.Debug.LogWarning($"IAP initialization failed: {status} - {message}");
                                _internetState.HasRemoveAds = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"IAP callback error: {ex.Message}");
                            _internetState.HasRemoveAds = false;
                        }
                        finally
                        {
                            completionSource.TrySetResult();
                        }
                    });

                    await completionSource.Task;
                }
            }
            catch (OperationCanceledException)
            {
                UnityEngine.Debug.LogWarning("IAP initialization timeout");
                _internetState.HasRemoveAds = false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"IAP initialization error: {ex.Message}");
                _internetState.HasRemoveAds = false;
            }
        }
    }
}