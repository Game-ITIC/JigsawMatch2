using System;
using Cysharp.Threading.Tasks;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Threading;

namespace Services
{
    public class RegionUpgradeService : IDisposable
    {
        private PlayableGraph playableGraph;
        private AnimationPlayableOutput playableOutput;
        private AnimationClipPlayable clipPlayable;

        private RegionModel _regionModel;
        public Animator animator;
        public AnimationClip animationClip;

        private CancellationTokenSource _cancellationTokenSource;

        public void Initialize(RegionModel regionModel)
        {
            _regionModel = regionModel;

            Debug.Log(_regionModel.CurrentLevelProgress);
            animator = regionModel._buildingsAnimationConfig.animator;
            animationClip = regionModel._buildingsAnimationConfig.animationClip;

            _cancellationTokenSource = new CancellationTokenSource();

            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

            clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
            playableOutput.SetSourcePlayable(clipPlayable);

            playableGraph.Play();
        }


        [Button]
        public void JumpToFrame(int frame)
        {
            float frameRate = animationClip.frameRate;
            double targetTime = frame / frameRate;

            clipPlayable.SetTime(targetTime);
            clipPlayable.SetSpeed(0); // Останавливаем
            playableGraph.Evaluate(); // Принудительно обновляем
        }

        [Button]
        public async UniTask PlayToFrame(int targetFrame)
        {
            float frameRate = animationClip.frameRate;
            double targetTime = targetFrame / frameRate;

            clipPlayable.SetSpeed(1);
            await StopAtFrame(targetTime, _cancellationTokenSource.Token);
        }

        [Button]
        private async UniTask StopAtFrame(double targetTime, CancellationToken cancellationToken = default)
        {
            while (clipPlayable.GetTime() < targetTime)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            clipPlayable.SetSpeed(0);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            playableGraph.Destroy();
        }
    }
}