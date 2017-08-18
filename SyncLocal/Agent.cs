using System;

using Library.Framework;
using Library.Synchronize;
using Library.Utility;

using Synchronization;

namespace SyncLocal
{
    public class Agent : IRequestQueue, IResponseQueue, ISoulBinder, IAgent
    {
        public delegate void ConnectedCallback();

        private event Action _ConnectEvent;

        private event Action _OnBreakEvent;

        private event Action<string, string> _OnErrorMethodEvent;

        private event Action<byte[], byte[]> _OnErrorVerifyEvent;

        public event ConnectedCallback OnConnectedEvent;

        private readonly AgentCore _Agent;

        private readonly GhostRequest _GhostRequest;

        private readonly SoulProvider _SoulProvider;

        private bool _Connected;

        private ISoulBinder _Binder => _SoulProvider;

        public Agent(IProtocol protocol)
        {
            _GhostRequest = new GhostRequest();
            _Agent = new AgentCore();
            _SoulProvider = new SoulProvider(this, this);
        }

        void IBootable.Launch()
        {
            _Launch();
        }

        void IBootable._Shutdown()
        {
            _Shutdown();
        }

        bool IUpdatable.Update()
        {
            _Update();

            return true;
        }

        event Action IAgent.BreakEvent
        {
            add => _OnBreakEvent += value;
            remove => _OnBreakEvent -= value;
        }

        event Action IAgent.ConnectEvent
        {
            add => _ConnectEvent += value;
            remove => _ConnectEvent += value;
        }

        event Action<string, string> IAgent.ErrorMethodEvent
        {
            add => _OnErrorMethodEvent += value;
            remove => _OnErrorMethodEvent -= value;
        }

        event Action<byte[], byte[]> IAgent.ErrorVerifyEvent
        {
            add => _OnErrorVerifyEvent += value;
            remove => _OnErrorVerifyEvent -= value;
        }

        long IAgent.Ping => _Agent.Ping;

        bool IAgent.Connected => _Connected;

        INotifier<T> IAgent.QueryNotifier<T>()
        {
            return _QueryProvider<T>();
        }

        void IAgent.Disconnect()
        {
            _Shutdown();
        }

        Value<bool> IAgent.Connect(string account, int password)
        {
            OnConnectedEvent();
            _Connected = true;
            return true;
        }

        event Action IRequestQueue.OnBreakEvent
        {
            add => _OnBreakEvent += value;
            remove => _OnBreakEvent -= value;
        }

        event InvokeMethodCallback IRequestQueue.OnInvokeMethodEvent
        {
            add => _GhostRequest.OnCallMethodEvent += value;
            remove => _GhostRequest.OnCallMethodEvent -= value;
        }

        void IRequestQueue.Update()
        {
            _Update();
        }

        void IResponseQueue.Push(ServerToClientOpCode code, Type package)
        {
            _Agent.OnResponse(code, package);
        }

        event Action ISoulBinder.OnBreakEvent
        {
            add => _OnBreakEvent += value;
            remove => _OnBreakEvent -= value;
        }

        void ISoulBinder.Return<TSoul>(TSoul soul)
        {
            _Binder.Return(soul);
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
            _Binder.Bind(soul);
        }

        private void _Unbind<TSoul>(TSoul soul)
        {
            _Binder.Unbind(soul);
        }

        private void _Launch()
        {
            throw new NotImplementedException();
        }

        private void _Shutdown()
        {
        }

        private INotifier<T> _QueryProvider<T>()
        {
            return _Agent.QueryProvider<T>();
        }
    }
}
