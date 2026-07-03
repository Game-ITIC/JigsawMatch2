using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class IronSourceInitializer : MonoBehaviour
{
    [SerializeField] private float maxWaitTime = 15f;

    private IronSourceManager _ironSourceManager;

    [Inject]
    private void Construct(IronSourceManager ironSourceManager)
    {
        _ironSourceManager = ironSourceManager;
    }

    public async UniTask<bool> WaitForIronSourceInit()
    {
        if(_ironSourceManager == null)
        {
            Debug.LogError($"{nameof(IronSourceInitializer)} was not injected with {nameof(IronSourceManager)}.", this);
            return false;
        }

        _ironSourceManager.InitializeLevelPlay();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Mathf.Max(0.1f, maxWaitTime)));

        try
        {
            await UniTask.WaitUntil(
                () => _ironSourceManager != null && _ironSourceManager.AreAdsReady(),
                cancellationToken: timeout.Token);
            return true;
        }
        catch(OperationCanceledException)
        {
            return false;
        }
    }
}
