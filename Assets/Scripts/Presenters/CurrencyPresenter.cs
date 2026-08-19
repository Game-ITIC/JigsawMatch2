using System;
using R3;
using Systems.CurrencySystem;
using Systems.CurrencySystem.Interfaces;
using VContainer.Unity;

namespace Presenters
{
    public class CurrencyPresenter : IInitializable, IDisposable
    {
        private readonly ICurrencyView _view;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencyType _type;

        private readonly CompositeDisposable _disposables = new();

        public CurrencyPresenter(
            ICurrencyView view,
            ICurrencyService currencyService,
            CurrencyType type)
        {
            _view = view;
            _currencyService = currencyService;
            _type = type;
        }

        public void Initialize()
        {
            if (_view == null || _currencyService == null)
            {
                return;
            }

            var observable = _currencyService.GetCurrencyObservable(_type);
            if (observable == null)
            {
                return;
            }

            observable
                .Subscribe(OnCurrencyUpdate)
                .AddTo(_disposables);

            OnCurrencyUpdate(observable.Value);
        }

        private void OnCurrencyUpdate(ICurrency currency)
        {
            if (currency != null && _view != null)
            {
                _view.SetAmount((int)currency.Value);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
