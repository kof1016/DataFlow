using Regulus.Utility;

using Synchronization;

using SyncLocal;

namespace Console
{
    internal class Application : WindowConsole
    {
        private readonly Library.Utility.Updater _Updater = new Library.Utility.Updater();

        private readonly IGhostQuerier _GhostQuerier;

        private readonly ISoulBinder _Binder;

        
        

        public Application()
        {
            var agent = new Agent();            
            _GhostQuerier = agent.GhostQuerier; // client
            _Binder = agent.Binder; // server
        }

        protected override void _Launch()
        {
            _Updater.Add(_GhostQuerier);
            _Updater.Add(new Logic(_Binder, Command, Viewer));
            _Updater.Add(new Visual(_GhostQuerier, Command, Viewer));
        }

        protected override void _Update()
        {
            _Updater.Working();
        }

        protected override void _Shutdown()
        {
            _Updater.Shutdown();
        }
    }
}
