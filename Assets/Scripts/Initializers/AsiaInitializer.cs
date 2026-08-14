using System;
using Cysharp.Threading.Tasks;
using Itic.Scopes;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Initializers
{
    public class AsiaInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        private readonly MainMenuPanel _mainMenuPanel;
        private readonly SceneLoader _sceneLoader;

        public AsiaInitializer(MenuView menuView, SceneLoader sceneLoader, IObjectResolver resolver)
        {
            _menuView = menuView;
            _sceneLoader = sceneLoader;
            _mainMenuPanel = resolver.ResolveOrDefault<MainMenuPanel>();
        }

        public void Initialize()
        {
            var playButton = _mainMenuPanel?.MenuActionPanel?.PlayButton?.Button
                             ?? FindButtonInScene("PlayButton", "Play Button", "Play", "StartGame", "Start Game");
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(StartGame);
            }
        }

        private void StartGame()
        {
            _sceneLoader.LoadGameAsync().Forget();
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
    }
}