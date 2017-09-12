using DataDefine;

using Library.Utility;

using Synchronization;
using Synchronization.Interface;

namespace GameLogic
{
    public class VerifyStage : IStage
    {
        public event Verify.DoneCallback OnDoneEvent;

        private readonly ISoulBinder _Binder;

        private readonly Verify _Verify;

        public VerifyStage(ISoulBinder binder, Verify verify)
        {
            _Binder = binder;
            _Verify = verify;
        }

        void IStage.Enter()
        {
            _Verify.OnDoneEvent += OnDoneEvent;

            _Binder.Bind<IVerify>(_Verify);
        }

        void IStage.Leave()
        {
            _Binder.Unbind<IVerify>(_Verify);

            _Verify.OnDoneEvent -= OnDoneEvent;
        }

        void IStage.Update()
        {
        }
    }
}
