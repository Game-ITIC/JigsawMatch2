using System;
using R3;
using UnityEngine;

namespace Monobehaviours
{
    public class GameEventDispatcher : MonoBehaviour, IGameEvents
    {
        public event Action OnEnterGame = delegate { };
        public event Action OnGameWon = delegate { };
        public event Action OnGameLost = delegate { };

        public void DispatchEnterGame() => OnEnterGame.Invoke();
        public void DispatchWin() => OnGameWon.Invoke();
        public void DispatchLose() => OnGameLost.Invoke();
    }
}