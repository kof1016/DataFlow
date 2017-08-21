using System;
using System.Reflection;

using Library.Synchronize;

namespace Synchronization
{
    public class GhostMethodHandler
    {
        private readonly IGhost _Ghost;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly IGhostRequest _Requester;

        public GhostMethodHandler(IGhost ghost, ReturnValueQueue return_value_queue, IGhostRequest requester)
        {
            _Ghost = ghost;
            _ReturnValueQueue = return_value_queue;
            _Requester = requester;
        }

        public void Run(MethodInfo info, object[] args, IValue return_value)
        {
            var package = new PackageCallMethod();
            package.EntityId = _Ghost.GetID();
            package.MethodName = info.Name;
            package.MethodParams = args;

            if (return_value != null)
                package.ReturnId = _ReturnValueQueue.PushReturnValue(return_value);

            _Requester.Request(ClientToServerOpCode.CALL_METHOD, package);
        }
    }
}