using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public class InternetChecker
    {
        private float _timeout = 3f;
        private string _testUrl = "https://www.google.com";

        private bool HasNetworkConnection()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public async UniTask<bool> HasInternetAccess()
        {
            if (!HasNetworkConnection())
            {
                Debug.LogWarning("[InternetChecker] No network reachability (internetReachability is NotReachable).");
                return false;
            }

            try
            {
                using var request = UnityWebRequest.Head(_testUrl);
                request.timeout = (int)_timeout;

                await request.SendWebRequest()
                    .ToUniTask()
                    .Timeout(TimeSpan.FromSeconds(_timeout));

                await UniTask.SwitchToMainThread();
                var success = request.result == UnityWebRequest.Result.Success;
                if (!success)
                {
                    Debug.LogWarning($"[InternetChecker] Head request to '{_testUrl}' failed: {request.error} (result: {request.result})");
                }
                return success;
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogWarning($"[InternetChecker] Failed to connect to '{_testUrl}' ({ex.GetType().Name}): {ex.Message}");
                return false;
            }
        }
    }
}