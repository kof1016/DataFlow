using DataDefine;

using Library.Framework;

using Regulus.Utility;

using Synchronization;

using IUpdatable = Library.Utility.IUpdatable;

namespace Console
{
    internal class Visual : IUpdatable
    {
        private readonly IGhostQuerier _GhostQuerier;
        private Command _Command;
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
                "Start",
                () =>
                {
                    _GhostQuerier.QueryNotifier<IVerify>().Supply += _SupplyVerify;
                });

            //_Command.Register("Shutdown", _Shutdown);


            //_GhostQuerier.QueryNotifier<IVerify>().Supply += _SupplyVerify;
            //_GhostQuerier.QueryNotifier<IVerify2>().Supply += _SupplyVerify2;
        }

        void IBootable.Shutdown()
        {
            _Shutdown();
        }

        private void _Shutdown()
        {
            _GhostQuerier.QueryNotifier<IVerify>().Supply -= _SupplyVerify;
            _GhostQuerier.QueryNotifier<IVerify2>().Supply -= _SupplyVerify2;
        }

        private void _SupplyVerify2(IVerify2 obj)
        {
            var result = obj.Login("1", "1");
        }

        private void _SupplyVerify(IVerify obj)
        {
            
            //_Command.Register<string,string>("LoginA [arg1,arg2]", _LoginA);
            _Command.RegisterLambda<Visual,string, string>(this , (instnace , arg1,arg2) => instnace._LoginA(arg1,arg2));
            _Command.Register("Login []", ()=>_Login(obj));
        }


        void _LoginA(string a ,string b)
        {
            
        }
        

        private void _Login(IVerify obj)
        {
            var result = obj.Login("1", "1");
            result.OnValueEvent += res =>
                {
                    if (res)
                    {
                        // login ok
                    }
                };
        }

        bool IUpdatable.Update()
        {
            return true;
        }
    }
}