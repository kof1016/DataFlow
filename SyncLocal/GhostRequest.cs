using System;
using System.Collections.Generic;

using Gateway.Serialization;

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

        private readonly ISerializer _Serializer;

        public GhostRequest(ISerializer serializer)
        {
            _Serializer = serializer;
            _Requests = new Queue<RequestPackage>();
        }

        void IGhostRequest.Request(ClientToServerOpCode code, byte[] args)
        {
            lock(_Requests)
            {
                _Requests.Enqueue(
                    new RequestPackage
                    {
                        Code = code,
                        Data = args
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

        private void _Apportion(ClientToServerOpCode code, byte[] args)
        {
            switch(code)
            {
                case ClientToServerOpCode.Ping:
                    OnPingEvent?.Invoke();
                    break;
                case ClientToServerOpCode.CallMethod:
                    {
                        var pkg = args.ToPackageData<PackageCallMethod>(_Serializer);

                        OnCallMethodEvent?.Invoke(pkg.EntityId, pkg.MethodId, pkg.ReturnId, pkg.MethodParams);
                    }

                    break;
                case ClientToServerOpCode.Release:
                    {
                        var pkg = args.ToPackageData<PackageRelease>(_Serializer);
                        
                        OnReleaseEvent?.Invoke(pkg.EntityId);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }
    }
}
