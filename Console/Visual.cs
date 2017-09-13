using DataDefine;

using Library.Framework;
using Library.Synchronize;

using Regulus.Utility;

using Synchronization;
using Synchronization.Interface;

using IUpdatable = Library.Utility.IUpdatable;

namespace Console
{
    public class Visual : IUpdatable
    {
        private readonly IGhostQuerier _GhostQuerier;
        private readonly Command _Command;
        private Regulus.Utility.Console.IViewer _Viewer;

        public Visual(IGhostQuerier ghost_querier,
            Command command, 
            Regulus.Utility.Console.IViewer viewer)
        {
            _GhostQuerier = ghost_querier;
            _Command = command;
            _Viewer = viewer;
        }

        void IBootable.Launch()
        {
            _Command.Register(
                "start",
                () =>
                {
                    _GhostQuerier.QueryNotifier<IVerify>().Supply += _SupplyVerify;
                });
        }

        void IBootable.Shutdown()
        {
            _Shutdown();
        }

        private void _Shutdown()
        {
            _GhostQuerier.QueryNotifier<IVerify>().Supply -= _SupplyVerify;
        }

        private void _SupplyVerify(IVerify obj)
        {
            // 直接呼叫
            //            var result = obj.Login("1", "1");
            //            result.OnValueEvent += (t) => { _Viewer.WriteLine($"回傳{t}"); };


            // command 使用方法1
            _Command.Register<string, string>(
                "m [a1, a2]", 
                (a1, a2) => { obj.Login(a1, a2); });




            // command 使用方法2
                        _Command.RegisterLambda<IVerify, string, string, Value<bool>>
                            (
                            obj, 
                            (instance, a1, a2) => instance.Login(a1, a2),
                            result => { _Viewer.WriteLine($"回傳{result}");});
        }

        private void Result_OnValueEvent(bool obj)
        {
            throw new System.NotImplementedException();
        }

        bool IUpdatable.Update()
        {
            return true;
        }
    }
}