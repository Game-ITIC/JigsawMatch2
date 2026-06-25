using System;
using Cysharp.Threading.Tasks;
using Views;

namespace Itic.Services
{
    public class ScreenService
    {
        private readonly LoadingScreenView _screenView;
        private bool _isVisible;

        public ScreenService(LoadingScreenView screenView)
        {
            _screenView = screenView;
        }

        public bool IsLoadingScreenVisible => _isVisible;

        public async UniTask ShowLoadingScreenAsync()
        {
            if (_isVisible)
            {
                await UniTask.CompletedTask;
                return;
            }

            _isVisible = true;
            _screenView.Show();
            await UniTask.CompletedTask;
        }

        public void SetLoadingProgress(float normalized)
        {
            if (!_isVisible)
            {
                return;
            }

            _screenView.SetProgress(normalized);
        }

        public async UniTask HideLoadingScreenAsync()
        {
            if (!_isVisible)
            {
                await UniTask.CompletedTask;
                return;
            }

            _screenView.SetProgress(1f);
            await UniTask.Delay(200);
            _screenView.Hide();
            _isVisible = false;
            await UniTask.CompletedTask;
        }
    }
}
