using System;
using System.Collections.Generic;

using Synchronization;
using Synchronization.Data;
using Synchronization.Interface;

namespace SyncLocal
{
    internal class GhostRequest : IGhostRequest
    {
        public event InvokeMethodCallback OnCallMethodEvent;

        public event Action OnPingEvent;

        public event Action<Guid> OnReleaseEvent;

        private readonly Queue<RequestPackage> _Requests;

        public GhostRequest()
        {
            _Requests = new Queue<RequestPackage>();
        }

        void IGhostRequest.Request(ClientToServerOpCode code, object arg)
        {
            lock(_Requests)
            {
                _Requests.Enqueue(
                    new RequestPackage
                    {
                        Code = code,
                        Data = arg
                    });
            }
        }

        public void Update()
        {
            var requests = new Queue<RequestPackage>();
            lock(_Requests)
            {
                while(_Requests.Count > 0)
                {
                    requests.Enqueue(_Requests.Dequeue());
                }
            }

            while(requests.Count > 0)
            {
                var request = requests.Dequeue();
                _Apportion(request.Code, request.Data);
            }
        }

        private void _Apportion(ClientToServerOpCode code, object arg)
        {
            switch(code)
            {
                case ClientToServerOpCode.Ping:
                    OnPingEvent?.Invoke();
                    break;
                case ClientToServerOpCode.CallMethod:
                    {
                        var pkg = arg as PackageCallMethod;
                        OnCallMethodEvent?.Invoke(pkg.EntityId, pkg.MethodName, pkg.ReturnId, pkg.MethodParams);
                    }

                    break;
                case ClientToServerOpCode.Release:
                    {
                        var pkg = arg as PackageRelease;
                        OnReleaseEvent?.Invoke(pkg.EntityId);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }
    }
}
