using UnityEngine;
using UnityEngine.UI;

namespace JuiceFresh.States
{
    public class GameOverState : GameStateBase
    {
        private GameObject _menuFailedUI;

        public GameOverState(LevelManager levelManager) : base(levelManager)
        {
        }

        public override void EnterState()
        {
            StopGameMusic();

            PlayGameOverSound();

            ShowFailedUI();

            LevelManager.TriggerOnLose();
        }

        public override void UpdateState()
        {
            // No specific update logic for game over state
        }

        public override void ExitState()
        {
            // Hide the failed UI if it's still visible
            if (_menuFailedUI != null && _menuFailedUI.activeSelf)
            {
                _menuFailedUI.SetActive(false);
            }
        }

        private void StopGameMusic()
        {
            MusicBase.Instance.GetComponent<AudioSource>().Stop();
        }

        private void PlayGameOverSound()
        {
            SoundBase.Instance.PlaySound(SoundBase.Instance.gameOver[0]);
        }

        private void ShowFailedUI()
        {
            _menuFailedUI = GameObject.Find("CanvasGlobal").transform.Find("MenuFailed").gameObject;
            _menuFailedUI.SetActive(true);

            SetupFailedMenu();
        }

        private void SetupFailedMenu()
        {
            // Get the score display and set it
            Text scoreText = _menuFailedUI.transform.Find("Score").GetComponent<Text>();
            if (scoreText != null)
            {
                scoreText.text = LevelManager.Score.ToString();
            }

            // Set up retry button
            Button retryButton = _menuFailedUI.transform.Find("ButtonReplay").GetComponent<Button>();
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => OnRetryClicked());
            }

            // Set up quit button
            Button quitButton = _menuFailedUI.transform.Find("ButtonQuit").GetComponent<Button>();
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(() => OnQuitClicked());
            }

            // Set up any continue mechanics if applicable
            SetupContinueOption();
        }

        private void SetupContinueOption()
        {
            // If the level allows continuing after failing
            if (levelManager.FailedCost > 0)
            {
                // Set up continue button
                Button continueButton = _menuFailedUI.transform.Find("ButtonContinue").GetComponent<Button>();
                if (continueButton != null)
                {
                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(() => OnContinueClicked());

                    // Update the cost text
                    Text costText = continueButton.transform.Find("Cost").GetComponent<Text>();
                    if (costText != null)
                    {
                        costText.text = levelManager.FailedCost.ToString();
                    }

                    // Enable/disable based on whether player has enough gems
                    bool canAfford = levelManager.CoinModel.Coins.Value >= levelManager.FailedCost;
                    continueButton.interactable = canAfford;
                }
            }
        }

        private void OnRetryClicked()
        {
            // Retry the current level
            levelManager.gameStatus = GameState.PrepareGame;
        }

        private void OnQuitClicked()
        {
            // Return to map
            levelManager.gameStatus = GameState.Map;
        }

        private void OnContinueClicked()
        {
            // Spend gems to continue
            InitScript.Instance.SpendGems(levelManager.FailedCost);

            // Add extra moves or time
            if (levelManager.limitType == LIMIT.MOVES)
            {
                levelManager.Limit += levelManager.ExtraFailedMoves;
            }
            else if (levelManager.limitType == LIMIT.TIME)
            {
                levelManager.Limit += levelManager.ExtraFailedSecs;
            }

            // Hide the failed UI
            _menuFailedUI.SetActive(false);

            // Return to playing state
            levelManager.gameStatus = GameState.Playing;
        }
    }
}