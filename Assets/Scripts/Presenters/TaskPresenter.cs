using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

namespace Presenters
{
    public class TaskPresenter : IInitializable, IDisposable
    {
        private readonly TaskPanel _taskPanel;
        private Button _taskButton;
        private readonly List<(Button Button, UnityAction Action)> _closeActions = new();

        public TaskPresenter(TaskPanel taskPanel, Button taskButton)
        {
            _taskPanel = taskPanel;
            _taskButton = taskButton;
        }

        public void Initialize()
        {
            if (_taskPanel == null) return;

            _taskPanel.HideImmediate();

            if (_taskButton == null)
            {
                _taskButton = FindButtonInScene("TaskButton", "Task Button", "Tasks Button", "Task Button 01", "Daily Button", "DailyButton");
            }

            if (_taskButton != null)
            {
                _taskButton.onClick.RemoveListener(OnTaskButtonClicked);
                _taskButton.onClick.AddListener(OnTaskButtonClicked);
            }
            else
            {
                Debug.LogWarning("[TaskPresenter] Task button could not be found in TopBarPanel or Scene.");
            }

            if (_taskPanel.CloseButtons != null)
            {
                foreach (var closeButton in _taskPanel.CloseButtons)
                {
                    if (closeButton == null) continue;

                    UnityAction closeAction = OnCloseButtonClicked;
                    closeButton.onClick.RemoveListener(closeAction);
                    closeButton.onClick.AddListener(closeAction);
                    _closeActions.Add((closeButton, closeAction));
                }
            }
        }

        private void OnTaskButtonClicked()
        {
            _taskPanel.Show();
        }

        private void OnCloseButtonClicked()
        {
            _taskPanel.Hide();
        }

        private static Button FindButtonInScene(params string[] names)
        {
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    var buttons = root.GetComponentsInChildren<Button>(true);
                    foreach (var button in buttons)
                    {
                        foreach (var name in names)
                        {
                            if (string.Equals(button.gameObject.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                return button;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (_taskButton != null)
            {
                _taskButton.onClick.RemoveListener(OnTaskButtonClicked);
            }

            foreach (var (button, action) in _closeActions)
            {
                if (button != null && action != null)
                {
                    button.onClick.RemoveListener(action);
                }
            }

            _closeActions.Clear();
        }
    }
}
