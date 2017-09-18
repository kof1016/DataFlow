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

        Value<bool> IVerify.Login(string id, string password)
        {
            var returnValue = new Value<bool>();

            returnValue.SetValue(id == "1" && password == "1");

            OnDoneEvent?.Invoke();
            return returnValue;
        }
    }
}
