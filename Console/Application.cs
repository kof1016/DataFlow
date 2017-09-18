using Regulus.Utility;

using Synchronization;
using Synchronization.Interface;

using SyncLocal;

using Updater = Library.Utility.Updater;

namespace Console
{
    public class Application : WindowConsole
    {
        private readonly ISoulBinder _Binder;

        private readonly IGhostQuerier _GhostQuerier;

        private readonly Updater _Updater = new Updater();

        private readonly Agent _Agent;
        public Application()
        {
            var agent = new Agent();
            _GhostQuerier = agent.GhostQuerier; // client
            _Binder = agent.Binder; // server
            _Agent = agent;
        }

        protected override void _Launch()
        {
            _Updater.Add(_Agent);
            _Updater.Add(new Logic(_Binder, Viewer));
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
