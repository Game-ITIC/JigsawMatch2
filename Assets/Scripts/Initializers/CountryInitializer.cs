using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Initializers
{
    public class CountryInitializer : IInitializable
    {
        private readonly MenuView _menuView;

        public CountryInitializer(MenuView menuView)
        {
            _menuView = menuView;
        }

        public void Initialize()
        {
            _menuView.Warmup();
            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            PlayerPrefs.SetInt("OpenLevel", 0);
            SceneManager.LoadScene("game");
        }
    }
}