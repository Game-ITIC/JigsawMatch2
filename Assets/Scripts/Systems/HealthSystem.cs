using System;
using R3;
using UnityEngine;
using Utils.Save;
using VContainer.Unity;

namespace Systems
{
    public class HealthSystem
    {
        private int _maxLives = 5;
        private float _regenTimeMinutes = 20f; // Время восстановления одной жизни в минутах
        
        // Приватные поля
        private ReactiveProperty<int> _currentLives = new();
        private DateTime _lastSaveTime;
        private bool _isRegenerating = false;


        public ReactiveProperty<int> CurrentLives => _currentLives;
        public int MaxLives => _maxLives;
        public bool CanPlay => _currentLives.Value > 0;

        public HealthSystem()
        {
            LoadData();
        }

        public bool TryUseLife()
        {
            if (_currentLives.Value <= 0)
            {
                return false;
            }

            _currentLives.Value--;
            SaveData();
            UpdateUI();


            if (!_isRegenerating && _currentLives.Value < _maxLives)
            {
                StartRegeneration();
            }

            return true;
        }

        public void AddLives(int amount)
        {
            _currentLives.Value = Mathf.Min(_currentLives.Value + amount, _maxLives);
            SaveData();
            UpdateUI();

            if (_currentLives.Value >= _maxLives)
            {
            }
        }

        public void RestoreAllLives()
        {
            _currentLives.Value = _maxLives;
            SaveData();
            UpdateUI();
        }


        private void StartRegeneration()
        {
            _isRegenerating = true;
            _lastSaveTime = DateTime.Now;
            SaveData();
        }


        public void UpdateRegeneration()
        {
            if (_currentLives.Value >= _maxLives)
            {
                return;
            }

            TimeSpan timePassed = DateTime.Now - _lastSaveTime;
            double minutesPassed = timePassed.TotalMinutes;

            int livesToAdd = Mathf.FloorToInt((float)(minutesPassed / _regenTimeMinutes));

            if (livesToAdd > 0)
            {
                _currentLives.Value = Mathf.Min(_currentLives.Value + livesToAdd, _maxLives);
                _lastSaveTime = _lastSaveTime.AddMinutes(livesToAdd * _regenTimeMinutes);
                SaveData();

                if (_currentLives.Value >= _maxLives)
                {
                    return;
                }
            }
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Life, _currentLives.Value);
            PlayerPrefs.SetString(PlayerPrefsKeys.LifeLastSavedTime, _lastSaveTime.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            _currentLives.Value = PlayerPrefs.GetInt(PlayerPrefsKeys.Life, _maxLives);

            string lastSaveTimeString = PlayerPrefs.GetString(PlayerPrefsKeys.LifeLastSavedTime, "");

            if (string.IsNullOrEmpty(lastSaveTimeString))
            {
                // Первый запуск
                _lastSaveTime = DateTime.Now;
                _currentLives.Value = _maxLives;
            }
            else
            {
                try
                {
                    long lastSaveTimeBinary = Convert.ToInt64(lastSaveTimeString);
                    _lastSaveTime = DateTime.FromBinary(lastSaveTimeBinary);

                    if (_currentLives.Value < _maxLives)
                    {
                        TimeSpan offlineTime = DateTime.Now - _lastSaveTime;
                        int livesToAdd = Mathf.FloorToInt((float)(offlineTime.TotalMinutes / _regenTimeMinutes));

                        if (livesToAdd > 0)
                        {
                            _currentLives.Value = Mathf.Min(_currentLives.Value + livesToAdd, _maxLives);
                            _lastSaveTime = _lastSaveTime.AddMinutes(livesToAdd * _regenTimeMinutes);
                        }
                    }
                }
                catch
                {
                    _lastSaveTime = DateTime.Now;
                    _currentLives.Value = _maxLives;
                }
            }

            SaveData();
        }

        #region UI

        private void UpdateUI()
        {
            // if (livesText != null)
            // {
            //     livesText.text = $"{currentLives}/{maxLives}";
            // }
            //
            // if (playButton != null)
            // {
            //     playButton.interactable = CanPlay;
            // }
            //
            // if (currentLives < maxLives && !isRegenerating)
            // {
            //     StartRegeneration();
            // }
        }

        #endregion
    }
}