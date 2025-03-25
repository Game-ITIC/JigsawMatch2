using UnityEngine;

namespace JuiceFresh.States
{
    public abstract class GameStateBase
    {
        protected LevelManager levelManager;

        public GameStateBase(LevelManager levelManager)
        {
            this.levelManager = levelManager;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}