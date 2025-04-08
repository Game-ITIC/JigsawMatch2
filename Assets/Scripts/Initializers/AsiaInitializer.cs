using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Initializers
{
    public class AsiaInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        
        public AsiaInitializer(MenuView menuView)
        {
            _menuView = menuView;
        }

        public void Initialize()
        {
            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            if (InitScript.lifes > 0)
            {
                SceneManager.LoadScene("game");
            }
        }
    }
}