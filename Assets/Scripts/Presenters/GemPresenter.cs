using System;
using Interfaces;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class GemPresenter : IInitializable, IDisposable
    {
        private readonly Models.GemModel _starModel;
        private readonly TextView _textView;

        private IDisposable _disposable;

        public GemPresenter(Models.GemModel starModel, TextView textView)
        {
            _starModel = starModel;
            _textView = textView;
        }

        public void Initialize()
        {
            _disposable = _starModel.Gems.Subscribe(
                (int newValue) => { _textView.SetText(newValue.ToString()); }
            );
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}