using System;

namespace Monobehaviours
{
    public interface IGameEvents
    {
        event Action OnEnterGame;
        event Action OnGameWon;
        event Action OnGameLost;

        void DispatchEnterGame();
        void DispatchWin();
        void DispatchLose();
    }
}