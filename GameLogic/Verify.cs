using System;

using DataDefine;

using Library.Synchronize;

namespace GameLogic
{
    public class Verify : IVerify
    {
        public delegate void DoneCallback();

        public event DoneCallback OnDoneEvent;

        public Verify()
        {
        }

        public int TestProperty
        {
            get
            {
                return 921;
            }
        }

        private event Action<bool> _TestEvent;

        event Action<bool> IVerify.TestEvent
        {
            add
            {
                this._TestEvent += value;
            }
            remove
            {
                this._TestEvent -= value;
            }
        }

        Value<bool> IVerify.Login(string id, string password)
        {
            var returnValue = new Value<bool>();

            returnValue.SetValue(id == "1" && password == "1");

            OnDoneEvent?.Invoke();
            _TestEvent(true);
            return returnValue;
        }
    }
}
