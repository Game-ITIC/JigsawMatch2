using Services;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Initializers
{
    public class AsiaInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        private readonly ISceneLoader _sceneLoader;

        public AsiaInitializer(MenuView menuView, ISceneLoader sceneLoader)
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

            _sceneLoader.LoadGameplayScene();
        }
    }
}