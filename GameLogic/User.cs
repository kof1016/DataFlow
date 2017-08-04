using System;

using DataDefine;

using Utility;

namespace GameLogic
{
    public class User : IUser, IAccountStatus
    {
        private event Action _OnKickEvent;

        private event OnQuit _OnQuitEvent;

        private event OnNewUser _OnVerifySuccessEvent;

        private readonly IAccountFinder _AccountFinder;

        private readonly ISoulBinder _Binder;

        private readonly StateMachine _Machine;

        private Account _Account;

        public User(ISoulBinder binder, IAccountFinder account_finder)
        {
            _Machine = new StateMachine();

            _Binder = binder;
            _AccountFinder = account_finder;
        }

        event Action IAccountStatus.OnKickEvent
        {
            add => _OnKickEvent += value;
            remove => _OnKickEvent -= value;
        }

        event OnNewUser IUser.OnVerifySuccessEvent
        {
            add => _OnVerifySuccessEvent += value;
            remove => _OnVerifySuccessEvent -= value;
        }

        event OnQuit IUser.OnQuitEvent
        {
            add => _OnQuitEvent += value;
            remove => _OnQuitEvent -= value;
        }

        void IUser.OnKick(Guid id)
        {
            if(_Account == null)
            {
                return;
            }

            if(_Account.Id != id)
            {
                return;
            }

            _OnKickEvent?.Invoke();

            _ToVerify();
        }

        void IBootable.Launch()
        {
            _Binder.OnBreakEvent += _Quit;

            _Binder.Bind<IAccountStatus>(this);

            _ToVerify();
        }

        void IBootable.Shutdown()
        {
            _Binder.Unbind<IAccountStatus>(this);

            _Machine.Termination();

            _Binder.OnBreakEvent -= _Quit;
        }

        bool IUpdatable.Update()
        {
            _Machine.Update();
            return true;
        }

        private void _Quit()
        {
            _OnQuitEvent?.Invoke();
        }

        private void _ToVerify()
        {
            var verify = _CreateVerify();

            _AddVerifyToStage(verify);
        }

        private Verify _CreateVerify()
        {
            _Account = null;
            return new Verify(_AccountFinder);
        }

        private void _AddVerifyToStage(Verify verify)
        {
            var stage = new VerifyStage(_Binder, verify);
            stage.OnDoneEvent += _VerifySuccess;

            _Machine.Push(stage);
        }

        private void _VerifySuccess(Account account)
        {
            _OnVerifySuccessEvent.Invoke(account.Id);
            _Account = account;
        }
    }
}
