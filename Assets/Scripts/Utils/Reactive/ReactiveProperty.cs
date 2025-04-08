using System;
using System.Collections.Generic;

namespace Utils.Reactive
{
    public class ReactiveProperty<T>
    {
        private T _value;
        private readonly List<Action<T>> _subscribers = new();

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    NotifySubscribers();
                }
            }
        }

        public ReactiveProperty()
        {
            _value = default;
        }

        public ReactiveProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            _subscribers.Add(observer);

            observer(_value);

            return new Subscription(() => Unsubscribe(observer));
        }

        private void Unsubscribe(Action<T> observer)
        {
            _subscribers.Remove(observer);
        }

        private void NotifySubscribers()
        {
            foreach (var subscriber in _subscribers.ToArray())
            {
                subscriber(_value);
            }
        }

        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribeAction;

            public Subscription(Action unsubscribeAction)
            {
                _unsubscribeAction = unsubscribeAction;
            }

            public void Dispose()
            {
                _unsubscribeAction?.Invoke();
            }
        }

        public static implicit operator T(ReactiveProperty<T> property) => property._value;
    }
}