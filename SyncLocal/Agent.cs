using System;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization;
using Synchronization.Interface;
using Synchronization.PreGenerated;

namespace SyncLocal
{
    public class Agent : ISoulBinder, IGhostQuerier, IUpdatable
    {
        private readonly IProtocol _Protocol;

        private readonly CommandBridge _CommandBridge;

        private readonly GhostProvider _GhostProvider;

        private readonly GhostRequest _GhostRequest;

        private readonly SoulProvider _SoulProvider;

        public readonly ISoulBinder Binder;

        public readonly IGhostQuerier GhostQuerier;

        public Agent(IProtocol protocol)
        {
            _Protocol = protocol;
            _GhostRequest = new GhostRequest(protocol.GetSerialize());
            _GhostProvider = new GhostProvider(_Protocol);

            _CommandBridge = new CommandBridge(_GhostProvider, _GhostRequest);
            _SoulProvider = new SoulProvider(_CommandBridge.RequestQueue, _CommandBridge.ResponseQueue, _Protocol);

            Binder = _SoulProvider;
            GhostQuerier = this;
            _Launch();
        }

        INotifier<T> IGhostQuerier.QueryNotifier<T>()
        {
            return _QueryProvider<T>();
        }

        event Action ISoulBinder.OnBreakEvent
        {
            add
            {
            }

            remove
            {
            }
        }

        void ISoulBinder.Return<TSoul>(TSoul soul)
        {
            Binder.Return(soul);
        }

        void ISoulBinder.Bind<TSoul>(TSoul soul)
        {
            _Bind(soul);
        }

        void ISoulBinder.Unbind<TSoul>(TSoul soul)
        {
            _Unbind(soul);
        }

        void IBootable.Launch()
        {
            // _Launch();
        }

        void IBootable.Shutdown()
        {
            _Shutdown();
        }

        bool IUpdatable.Update()
        {
            _Update();

            return true;
        }

        private void _Update()
        {
            _SoulProvider.Update();
            _GhostRequest.Update();
        }

        private void _Bind<TSoul>(TSoul soul)
        {
            Binder.Bind(soul);
        }

        private void _Unbind<TSoul>(TSoul soul)
        {
            Binder.Unbind(soul);
        }

        private void _Launch()
        {
            // _GhostRequest.OnPingEvent += _OnRequestPing;
            _GhostRequest.OnReleaseEvent += _SoulProvider.Unbind;

            _GhostProvider.Initial(_GhostRequest);
        }

        private INotifier<T> _QueryProvider<T>()
        {
            return _GhostProvider.QueryProvider<T>();
        }

        private void _Shutdown()
        {
            _CommandBridge.Break();

            _GhostProvider.Finial();

            _GhostRequest.OnReleaseEvent -= _SoulProvider.Unbind;
        }
    }
}
