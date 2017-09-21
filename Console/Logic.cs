using System;

using DataDefine;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization.Interface;

namespace Console
{
    internal class Logic : IUpdatable, IVerify, IPlayer
    {
        private event Action<Move> OnMoveEvent;

        private readonly ISoulBinder _Binder;

        private readonly Regulus.Utility.Console.IViewer _Viewer;

        // private Center _Center;
        public Logic(
            ISoulBinder binder,
            Regulus.Utility.Console.IViewer viewer)
        {
            _Binder = binder;
            _Viewer = viewer;
        }

        Value<bool> IPlayer.IsMain()
        {
            return new Value<bool>(true);
        }

        event Action<Move> IPlayer.MoveEvent
        {
            add => OnMoveEvent += value;
            remove => OnMoveEvent -= value;
        }

        void IBootable.Launch()
        {
            _Viewer.WriteLine("\nTerry Test");

            _Binder.Bind<IVerify>(this);

            _Binder.Bind<IPlayer>(this);
        }

        void IBootable.Shutdown()
        {
            _Binder.Unbind<IVerify>(this);
        }

        bool IUpdatable.Update()
        {
            return true;
        }

        int IVerify.TestProperty
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
                _TestEvent += value;
            }
            remove
            {
                _TestEvent -= value;
            }
        }

        Value<bool> IVerify.Login(string id, string password)
        {
            _Viewer.WriteLine($"logic 收到 id = {id} password = {password}");

            var returnValue = new Value<bool>();

            var result = id == "1" && password == "1";

            returnValue.SetValue(result);
            _TestEvent(true);
            return returnValue;
        }
    }
}
