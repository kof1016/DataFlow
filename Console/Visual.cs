using System;
using System.CodeDom;

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
        private readonly Regulus.Utility.Console.IViewer _Viewer;

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
            obj.Login("1", "1");
            

            _Viewer.WriteLine($"property{obj.TestProperty}");
            obj.TestEvent += (result) =>
            {
                _Viewer.WriteLine($"event {result}");
            };


            // command 使用方法1
            //            _Command.Register<string, string>(
            //                "m [a1, a2]", 
            //                (a1, a2) => { obj.Login(a1, a2); });

            // command 使用方法2


            _Command.RegisterLambda<IVerify, string, string, Value<bool>>
                    (obj,
                     (instance, a1, a2) => instance.Login(a1, a2),
                     _ReturnValue);
        }

        private void _ReturnValue(Value<bool> return_value)
        {
            return_value.OnValueEvent += result =>
                {
                    _Viewer.WriteLine("==Visual Show==");

                    _Viewer.WriteLine($"Login result = {result}");
                };
        }

        private void Instance_OnMoveEvent(string arg1, string arg2)
        {
            throw new NotImplementedException();
        }

        private void Instance_OnMoveEvent2()
        {
            throw new NotImplementedException();
        }

        private void _ReturnValuePlayer(Action obj)
        {
            throw new NotImplementedException();
        }

        

        bool IUpdatable.Update()
        {
            return true;
        }
    }
}