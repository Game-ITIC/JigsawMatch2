using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class IronSourceInitializer : MonoBehaviour
{
    [SerializeField] private float maxWaitTime = 15f; // Максимальное время ожидания, секунд

    public async UniTask<bool> WaitForIronSourceInit()
    {
        var isReady = false;
        IronSourceManager.Instance.InitializeLevelPlay();

        var checkAdsTask = UniTask.WaitUntil(() =>
            IronSourceManager.Instance != null &&
            IronSourceManager.Instance.AreAdsReady());

        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(maxWaitTime));

        int completedTaskIndex = await UniTask.WhenAny(checkAdsTask, timeoutTask);

        if (completedTaskIndex == 0)
        {
            Debug.Log("IronSource готов, переходим в MainMenu");
            isReady = true;
        }
        else
        {
            Debug.LogWarning("Превышено время ожидания IronSource");
            isReady = false;
        }

        return isReady;
    }
}