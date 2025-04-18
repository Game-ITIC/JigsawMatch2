using Cysharp.Threading.Tasks;
using Views;

namespace Itic.Services
{
    public class ScreenService
    {
        private readonly LoadingScreenView _screenView;

        public ScreenService(LoadingScreenView screenView)
        {
            _screenView = screenView;
        }

        public async UniTask ShowLoadingScreenAsync()
        {
            _screenView.Show();
            await UniTask.CompletedTask;
        }

        public async UniTask HideLoadingScreenAsync()
        {
            _screenView.Hide();
            await UniTask.CompletedTask;
        }
    }
}