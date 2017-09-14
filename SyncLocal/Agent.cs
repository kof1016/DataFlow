using System;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization;
using Synchronization.Data;
using Synchronization.Interface;

namespace SyncLocal
{
    public class Agent : ISoulBinder, IGhostQuerier , IUpdatable
    {
        public delegate void ConnectedCallback();

        

        public event ConnectedCallback OnConnectedEvent;

        private readonly CommandBridge _CommandBridge;

        private readonly GhostProvider _GhostProvider;

        private readonly GhostRequest _GhostRequest;

        private readonly SoulProvider _SoulProvider;

        private bool _Connected;

        public readonly ISoulBinder Binder;

        public readonly IGhostQuerier GhostQuerier;

        public Agent()
        {
            _GhostRequest = new GhostRequest();
            _GhostProvider = new GhostProvider();

            _CommandBridge = new CommandBridge(_GhostProvider, _GhostRequest);
            _SoulProvider = new SoulProvider(_CommandBridge.RequestQueue, _CommandBridge.ResponseQueue);

            Binder = _SoulProvider;
            GhostQuerier = this;
            _Launch();
        }

        void IBootable.Launch()
        {
            //_Launch();
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
            // _GhostProvider.OnErrorVerifyEvent -= _OnErrorVerifyEvent;
            

            _Connected = false;

            _CommandBridge.Break();

            _GhostProvider.Finial();

            //_GhostRequest.OnPingEvent -= _OnRequestPing;
            _GhostRequest.OnReleaseEvent -= _SoulProvider.Unbind;
        }

        private void _OnRequestPing()
        {
            _GhostProvider.OnResponse(ServerToClientOpCode.Ping, new object[0]);
        }

        INotifier<T> IGhostQuerier.QueryNotifier<T>()
        {
            return _QueryProvider<T>();
        }
    }
}
