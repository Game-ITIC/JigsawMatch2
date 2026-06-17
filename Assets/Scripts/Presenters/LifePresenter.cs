using System;
using Interfaces;
using R3;
using Systems;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class LifePresenter : IInitializable, IDisposable
    {
        private readonly HealthSystem _healthSystem;
        private readonly TextView _textView;
        private readonly bool _showLifeCount;

        private CompositeDisposable _disposable = new();
        private string _lastText;

        public LifePresenter(HealthSystem healthSystem, TextView textView)
        {
            _healthSystem = healthSystem;
            _textView = textView;
            _showLifeCount = IsLifeCountText(textView.gameObject.name);
        }

        public void Initialize()
        {
            _healthSystem.CurrentLives.Subscribe(_ => UpdateLifeText()).AddTo(_disposable);

            Observable.EveryUpdate(UnityFrameProvider.EarlyUpdate)
                .Subscribe(_ =>
                           {
                               _healthSystem.UpdateRegeneration();
                               UpdateLifeText();
                           })
                .AddTo(_disposable);
        }

        private void UpdateLifeText()
        {
            var lifeText = _showLifeCount
                ? _healthSystem.CurrentLives.Value.ToString()
                : _healthSystem.GetLifeStatusText();

            if (_lastText == lifeText)
            {
                return;
            }

            _lastText = lifeText;
            _textView.SetText(lifeText);
        }

        private static bool IsLifeCountText(string objectName)
        {
            return string.Equals(objectName, "CountHelth", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(objectName, "CountHealth", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(objectName, "Life Count", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(objectName, "Lives Count", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
