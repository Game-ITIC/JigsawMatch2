using System;
using Interfaces;
using R3;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class CoinPresenter : IInitializable, IDisposable
    {
        private readonly Models.CoinModel _coinModel;
        private readonly TextView _textView;

        private CompositeDisposable _disposable = new();

        public CoinPresenter(Models.CoinModel coinModel, TextView textView)
        {
            _coinModel = coinModel;
            _textView = textView;
        }

        public void Initialize()
        {
            _coinModel.Coins.Subscribe(
                (int newValue) => { _textView.SetText(newValue.ToString()); }
            ).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}