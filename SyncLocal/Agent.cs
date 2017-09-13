using System;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization;
using Synchronization.Data;
using Synchronization.Interface;

namespace SyncLocal
{
    public class Agent : ISoulBinder, IGhostQuerier
    {
        public delegate void ConnectedCallback();

        private event Action _OnConnectEvent;

        private event Action<string, string> _OnErrorMethodEvent;

        private event Action<string, string> _OnErrorVerifyEvent;

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

        event Action IGhostQuerier.BreakEvent
        {
            add => _CommandBridge.RequestQueue.OnBreakEvent += value;
            remove => _CommandBridge.RequestQueue.OnBreakEvent -= value;
        }

        event Action IGhostQuerier.ConnectEvent
        {
            add => _OnConnectEvent += value;
            remove => _OnConnectEvent += value;
        }

        event Action<string, string> IGhostQuerier.ErrorMethodEvent
        {
            add => _OnErrorMethodEvent += value;
            remove => _OnErrorMethodEvent -= value;
        }

        event Action<string, string> IGhostQuerier.ErrorVerifyEvent
        {
            add => _OnErrorVerifyEvent += value;
            remove => _OnErrorVerifyEvent -= value;
        }

        long IGhostQuerier.Ping => _GhostProvider.Ping;

        bool IGhostQuerier.Connected => _Connected;

        INotifier<T> IGhostQuerier.QueryNotifier<T>()
        {
            return _QueryProvider<T>();
        }

        void IGhostQuerier.Disconnect()
        {
            _Shutdown();
        }

        Value<bool> IGhostQuerier.Connect(string ip_address, int password)
        {
            OnConnectedEvent();
            _Connected = true;
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

            _GhostProvider.OnErrorMethodEvent += _OnErrorMethodEvent;
            _GhostProvider.OnErrorVerifyEvent += _OnErrorVerifyEvent;
            _GhostProvider.Initial(_GhostRequest);
        }

        private INotifier<T> _QueryProvider<T>()
        {
            return _GhostProvider.QueryProvider<T>();
        }

        private void _Shutdown()
        {
            // _GhostProvider.OnErrorVerifyEvent -= _OnErrorVerifyEvent;
            _GhostProvider.OnErrorMethodEvent -= _OnErrorMethodEvent;

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
    }
}
