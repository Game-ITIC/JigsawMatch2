using UnityEngine;

namespace JuiceFresh.States
{
    public class PrepareBoostsState : GameStateBase
    {
        private ILevelManagerActions _levelManagerActions;
        public PrepareBoostsState(LevelManager levelManager) : base(levelManager)
        {
            _levelManagerActions = levelManager;
        }

        public override void EnterState()
        {
            SetPreBoosts();
        }

        public override void UpdateState()
        {
            // No continuous update logic needed for this state
        }

        public override void ExitState()
        {
            // Nothing specific to clean up
        }

        private void SetPreBoosts()
        {
            bool noBoosts = true;

            // Apply colorful bomb boost if available
            if (levelManager.BoostColorfullBomb > 0)
            {
                ApplyColorfulBombBoost();
                noBoosts = false;
            }

            // Apply striped boost if available
            if (levelManager.BoostStriped > 0)
            {
                ApplyStripedBoost();
                noBoosts = false;
                // Transition directly to Playing state
                levelManager.gameStatus = GameState.Playing;
            }

            // If no boosts were applied, transition to Playing state
            if (noBoosts)
            {
                levelManager.gameStatus = GameState.Playing;
            }
        }

        private void ApplyColorfulBombBoost()
        {
            // Create colorful_mix effect
            GameObject colorMix = Object.Instantiate(Resources.Load("Prefabs/Effects/colorful_mix")) as GameObject;
            colorMix.transform.position = Vector3.zero + Vector3.up * -5f;

            // Reset the boost counter
            levelManager.BoostColorfullBomb = 0;
        }

        private void ApplyStripedBoost()
        {
            // Apply striped boost to random items
            foreach (Item item in _levelManagerActions.GetRandomItems(levelManager.BoostStriped))
            {
                // Randomly select horizontal or vertical stripe
                item.nextType = (ItemsTypes)Random.Range(1, 3);
                item.ChangeType();
            }

            // Reset the boost counter
            levelManager.BoostStriped = 0;
        }
    }
}