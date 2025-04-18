using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Itic.Scopes
{
    [UsedImplicitly]
    public class InitializationQueue
    {
        private readonly Queue<Func<UniTaskVoid>> _firedTaskQueue = new();
        private readonly Queue<Func<UniTask>> _taskQueue = new();
        private readonly Queue<Action> _actionQueue = new();

        public async UniTask Execute()
        {
            for (int i = 0, length = _taskQueue.Count; i < length; i++)
            {
                await _taskQueue.Dequeue().Invoke();
            }

            for (int i = 0, length = _firedTaskQueue.Count; i < length; i++)
            {
                _firedTaskQueue.Dequeue().Invoke().Forget();
            }

            for (int i = 0, length = _actionQueue.Count; i < length; i++)
            {
                _actionQueue.Dequeue().Invoke();
                await UniTask.DelayFrame(2);
            }
        }

        public void Enqueue(Func<UniTask> task)
        {
            _taskQueue.Enqueue(task);
        }

        public void EnqueueFiredTask(Func<UniTaskVoid> task)
        {
            _firedTaskQueue.Enqueue(task);
        }

        public void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }
    }
}