using System;

using Synchronization;
using Synchronization.Data;
using Synchronization.Interface;

namespace SyncLocal
{
    internal class CommandBridge : IRequestQueue, IResponseQueue
    {
        private event Action _OnBreakEvent;

        private readonly GhostProvider _GhostProvider;

        private readonly GhostRequest _GhostRequest;

        public readonly IRequestQueue RequestQueue;

        public readonly IResponseQueue ResponseQueue;

        public CommandBridge(GhostProvider ghost_provider, GhostRequest ghost_request)
        {
            RequestQueue = this;
            ResponseQueue = this;
            _GhostProvider = ghost_provider;
            _GhostRequest = ghost_request;
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
        }

        void IResponseQueue.Push(ServerToClientOpCode code, object package)
        {
            _GhostProvider.OnResponse(code, package);
        }

        public void Break()
        {
            _OnBreakEvent?.Invoke();
        }
    }
}
