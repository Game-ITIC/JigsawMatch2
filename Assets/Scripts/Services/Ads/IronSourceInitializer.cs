using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class IronSourceInitializer : MonoBehaviour
{
    [SerializeField] private float maxWaitTime = 15f;

    public async UniTask<bool> WaitForIronSourceInit()
    {
        IronSourceManager.Instance.InitializeLevelPlay();

        var checkAdsTask = UniTask.WaitUntil(() =>
            IronSourceManager.Instance != null &&
            IronSourceManager.Instance.AreAdsReady());

        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(maxWaitTime));

        int completedTaskIndex = await UniTask.WhenAny(checkAdsTask, timeoutTask);

        return completedTaskIndex == 0;
    }
}
