using System;
using Interfaces;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class StarPresenter : IInitializable, IDisposable
    {
        private readonly Models.StarModel _starModel;
        private readonly TextView _textView;

        private IDisposable _disposable;

        public StarPresenter(Models.StarModel starModel, TextView textView)
        {
            _starModel = starModel;
            _textView = textView;
        }

        public void Initialize()
        {
            _disposable = _starModel.Stars.Subscribe(
                (int newValue) => { _textView.SetText(newValue.ToString()); }
            );
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}