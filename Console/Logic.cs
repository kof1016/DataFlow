using System;

using DataDefine;

using GameLogic.Play;

using Library.Framework;
using Library.Synchronize;

using Regulus.Utility;

using Synchronization;
using Synchronization.Interface;

using IUpdatable = Library.Utility.IUpdatable;

namespace Console
{
    internal class Logic : IUpdatable, IVerify, IPlayer
    {
        private readonly ISoulBinder _Binder;

        private Command _Command;

        private readonly Regulus.Utility.Console.IViewer _Viewer;

        private Center _Center;

        public Logic(
            ISoulBinder binder,
            Command command,
            Regulus.Utility.Console.IViewer viewer)
        {
            _Binder = binder;
            _Command = command;
            _Viewer = viewer;
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

        Value<bool> IVerify.Login(string id, string password)
        {
            _Viewer.WriteLine($"logic 收到 id = {id} password = {password}");

            var returnValue = new Value<bool>();

            var result = id == "1" && password == "1";

            returnValue.SetValue(result);

            return returnValue;
        }

        public bool Main { get; }

        public bool IsMain()
        {
            throw new NotImplementedException();
        }

        Value<bool> IPlayer.IsMain()
        {
            throw new NotImplementedException();
        }

        public event Action<Move> MoveEvent;
    }
}
