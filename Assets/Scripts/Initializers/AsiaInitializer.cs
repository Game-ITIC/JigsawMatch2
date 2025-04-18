using Cysharp.Threading.Tasks;
using Itic.Scopes;
using Services;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Initializers
{
    public class AsiaInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        private readonly SceneLoader _sceneLoader;

        public AsiaInitializer(MenuView menuView, SceneLoader sceneLoader)
        {
            _menuView = menuView;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            // if (InitScript.lifes > 0)
            // {
                // SceneManager.LoadScene("game");
                       
            // }

            _sceneLoader.LoadGameAsync().Forget();
        }
    }
}