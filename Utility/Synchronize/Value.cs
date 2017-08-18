using System;

namespace Library.Synchronize
{
    public class Value<T> : IValue
    {
        private event Action<T> _OnValueEvent;

        public event Action<T> OnValueEvent
        {
            add
            {
                _OnValueEvent += value;
                if(_Empty == false)
                {
                    value(_Value);
                }
            }

            remove => _OnValueEvent -= value;
        }

        private bool _Empty = true;

        private readonly bool _Interface;

        private T _Value;

        public static Value<T> Empty => default(T);

        public Value()
        {
            _Interface = typeof(T).IsInterface;
        }

        public Value(T val) : this()
        {
            _Empty = false;
            _Value = val;

            _OnValueEvent?.Invoke(_Value);
        }

        object IValue.GetObject()
        {
            return _Value;
        }

        void IValue.SetValue(object val)
        {
            SetValue((T)val);
        }

        void IValue.QueryValue(Action<object> action)
        {
            if (_Empty == false)
            {
                action.Invoke(_Value);
            }
            else
            {
                OnValueEvent += obj => action.Invoke(obj);
            }
        }

        void IValue.SetValue(IGhost ghost)
        {
            SetValue((T)ghost);
        }

        bool IValue.IsInterface()
        {
            return _Interface;
        }

        Type IValue.GetObjectType()
        {
            return typeof(T);
        }

        public static implicit operator Value<T>(T value)
        {
            return new Value<T>(value);
        }

        public void SetValue(T val)
        {
            if (_Empty == false)
            {
                throw new Exception("­«ÂÐªº set value");
            }
            _Empty = false;

            _Value = val;

            _OnValueEvent?.Invoke(_Value);
        }
    }
}
