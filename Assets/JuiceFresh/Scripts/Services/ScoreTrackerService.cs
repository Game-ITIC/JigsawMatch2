using UnityEngine;

namespace JuiceFresh.Scripts
{
    public class ScoreTrackerService
    {
        private readonly LevelManager _levelManager;

        public ScoreTrackerService(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void PopupScore(int value, Vector3 pos, int color)
        {
            LevelManager.Score += value;
            UpdateBar();
            CheckStars();

            // BoardMechanicsService checks win/lose after destruction and falling finish.
            // Switching to PreWin here would leave the completion flow waiting on an active board process.
            _levelManager.ScorePopupTweenSpawner?.Show(value, pos, color, _levelManager.scoresColors);
        }

        void UpdateBar()
        {
            if (ProgressBarScript.Instance == null)
                return;

            ProgressBarScript.Instance.SetProgress(LevelManager.Score, _levelManager.star3);
        }

        void CheckStars()
        {
            int earnedStars = LevelManager.Score >= _levelManager.star3 ? 3
                : LevelManager.Score >= _levelManager.star2 ? 2
                : LevelManager.Score >= _levelManager.star1 ? 1
                : 0;
            _levelManager.stars = Mathf.Max(_levelManager.stars, earnedStars);
        }
    }
}
