using UnityEngine;

namespace JuiceFresh.States
{
    public abstract class GameStateBase
    {
        protected LevelManager levelManager;
        protected string stateName;

        public GameStateBase(LevelManager levelManager)
        {
            this.levelManager = levelManager;
            this.stateName = this.GetType().Name;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        
        // Debug method to be called from PlayingState
        protected void DebugSelectionStatus()
        {
            if (levelManager.destroyAnyway.Count > 0)
            {
                Debug.Log($"[{stateName}] Currently selected items: {levelManager.destroyAnyway.Count}");
                string itemInfo = "";
                foreach (Item item in levelManager.destroyAnyway)
                {
                    if (item != null)
                    {
                        itemInfo += $"Item at ({item.square.col},{item.square.row}) Color: {item.color}, ";
                    }
                }
                Debug.Log($"[{stateName}] Item details: {itemInfo}");
            }
        }
    }
}