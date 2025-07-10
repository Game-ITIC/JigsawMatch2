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

        private CompositeDisposable _disposable = new();

        public LifePresenter(HealthSystem healthSystem, TextView textView)
        {
            _healthSystem = healthSystem;
            _textView = textView;
        }

        public void Initialize()
        {
            _healthSystem.CurrentLives.Subscribe((int newValue) => { _textView.SetText(newValue.ToString()); }
            ).AddTo(_disposable);

            Observable.EveryUpdate(UnityFrameProvider.EarlyUpdate).Subscribe(_ => _healthSystem.UpdateRegeneration()
            ).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}