using System.Reflection;

using Regulus.Utility;

using Synchronization;
using Synchronization.Interface;
using Synchronization.PreGenerated;

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
            var protocol = _CreateProtocol("MessagingGatewayDataDefine.Protocol.dll", "DataDefine.Protocol");

            var agent = new Agent(protocol);
            _GhostQuerier = agent.GhostQuerier; // client
            _Binder = agent.Binder; // server
            _Agent = agent;
        }

        private IProtocol _CreateProtocol(string protocol_path, string protocol_type_name)
        {
            
            var assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(protocol_path) );


            var instance = assembly.CreateInstance(protocol_type_name);
            return instance as IProtocol;
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
