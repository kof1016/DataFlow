using DataDefine;

using Library.Synchronize;

namespace GameLogic
{
    public class Verify : IVerify
    {
        public delegate void DoneCallback(Account account);

        public event DoneCallback OnDoneEvent;

        private readonly IAccountFinder _AccountFinder;

        public Verify(IAccountFinder accountFinder)
        {
            _AccountFinder = accountFinder;
        }

        Value<bool> IVerify.Login(string id, string password)
        {
            var returnValue = new Value<bool>();

            var val = _AccountFinder.FindAccountByName(id);
            val.OnValueEvent += account =>
                {
                    if(account != null && account.IsPassword(password))
                    {
                        OnDoneEvent?.Invoke(account);
                        returnValue.SetValue(true);
                    }
                    else
                    {
                        returnValue.SetValue(false);
                    }
                };

            return returnValue;
        }
    }
}
