using DataDefine;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization;

namespace Console
{
    internal class Logic : IUpdatable, IVerify, IVerify2
    {
        private readonly ISoulBinder _Binder;
        private Regulus.Utility.Command _Command;
        private Regulus.Utility.Console.IViewer _Viewer;

     //   private readonly Move _Move;

        public Logic(
            ISoulBinder binder,
            Regulus.Utility.Command command,
            Regulus.Utility.Console.IViewer viewer)
        {
         //   _Move = new Move();
            _Binder = binder;
            _Command = command;
            _Viewer = viewer;

            _Viewer.WriteLine("\nTerry Test");
        }

        void IBootable.Launch()
        {
            //_Command.Register("Verify", () => { _Binder.Bind<IVerify>(this); });
         
            //   _Binder.Bind<IMove>(_Move);

            //_Binder.Bind<IVerify2>(this);
            _Binder.Bind<IVerify>(this);
        }

        void IBootable.Shutdown()
        {
           // _Binder.Unbind<IMove>(_Move);
            _Binder.Unbind<IVerify2>(this);
            _Binder.Unbind<IVerify>(this);
        }

        bool IUpdatable.Update()
        {
            return true;
        }

        Value<bool> IVerify.Login(string id, string password)
        {
            return true;
        }

        Value<bool> IVerify2.Login(string id, string password)
        {
            return true;
        }
    }
}