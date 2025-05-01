using System;
using Interfaces;
using R3;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class StarPresenter : IInitializable, IDisposable
    {
        private readonly Models.StarModel _starModel;
        private readonly TextView _textView;

        private CompositeDisposable _disposable = new();

        public StarPresenter(Models.StarModel starModel, TextView textView)
        {
            _starModel = starModel;
            _textView = textView;
        }

        public void Initialize()
        {
            _starModel.Stars.Subscribe(
                (int newValue) => { _textView.SetText(newValue.ToString()); }
            ).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}