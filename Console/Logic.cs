using DataDefine;

using Library.Framework;
using Library.Synchronize;

using Regulus.Utility;

using Synchronization;
using Synchronization.Interface;

using SyncLocal;

using IUpdatable = Library.Utility.IUpdatable;

namespace Console
{
    internal class Logic : IUpdatable, IVerify
    {
        private readonly ISoulBinder _Binder;

        private Command _Command;

        private readonly Regulus.Utility.Console.IViewer _Viewer;

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
            //var val = Agent.Connect(ip, port);
            //val.OnValue += _ConnectResult;
            //return val;
            _Viewer.WriteLine($"logic 收到 id = {id} password = {password}");

            return new Value<bool>();
        }
    }
}
