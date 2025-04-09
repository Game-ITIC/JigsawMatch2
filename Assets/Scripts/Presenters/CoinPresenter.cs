using System;
using Interfaces;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class CoinPresenter : IInitializable, IDisposable
    {
        private readonly Models.CoinModel _starModel;
        private readonly TextView _textView;

        private IDisposable _disposable;

        public CoinPresenter(Models.CoinModel starModel, TextView textView)
        {
            _starModel = starModel;
            _textView = textView;
        }

        public void Initialize()
        {
            _disposable = _starModel.Coins.Subscribe(
                (int newValue) => { _textView.SetText(newValue.ToString()); }
            );
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}