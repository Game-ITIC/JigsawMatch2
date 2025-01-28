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
            if (!HasNetworkConnection()) return false;

            try
            {
                using var request = UnityWebRequest.Head(_testUrl);
                request.timeout = (int)_timeout;

                await request.SendWebRequest()
                    .ToUniTask()
                    .Timeout(TimeSpan.FromSeconds(_timeout));

                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}