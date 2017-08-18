using System;
using System.Reflection;

using Library.Synchronize;

namespace Synchronization
{
    public class GhostMethodHandler
    {
        private readonly IGhost _Ghost;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly object _Package;

        private readonly IGhostRequest _Requester;

        private readonly MemberMap _MemberMap = new MemberMap();



        public GhostMethodHandler(IGhost ghost, ReturnValueQueue return_value_queue, object protocol, IGhostRequest requester)
        {
            _Ghost = ghost;
            _ReturnValueQueue = return_value_queue;
            _Package = protocol;
            _Requester = requester;
        }

        public void Run(MethodInfo info, object[] args, IValue return_value)
        {
            var map = _MemberMap;
            var method = map.GetMethod(info);

            var package = new PackageCallMethod();
            package.EntityId = _Ghost.GetID();
            package.MethodId = method;
            package.MethodParams = args

            if (return_value != null)
                package.ReturnId = _ReturnValueQueue.PushReturnValue(return_value);

            _Requester.Request(ClientToServerOpCode.CallMethod, package.ToBuffer(serialize));
        }
    }
}