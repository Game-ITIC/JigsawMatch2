using Cysharp.Threading.Tasks;
using Itic.Scopes;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Initializers
{
    public class CountryInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        private readonly SceneLoader _sceneLoader;

        public CountryInitializer(MenuView menuView, SceneLoader sceneLoader)
        {
            _menuView = menuView;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            Debug.Log("I'm here in country");
            _menuView.Warmup();
            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);
            
            
            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            _menuView.StartGameText.SetText("LEVEL " + nextLevel);
        }

        private void StartGame()
        {
            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            PlayerPrefs.SetInt("OpenLevel", nextLevel);
            // SceneManager.LoadScene("game");
            _sceneLoader.LoadGameAsync().Forget();
        }
    }
}