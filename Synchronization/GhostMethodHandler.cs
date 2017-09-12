using System.Reflection;

using Library.Synchronize;

using Synchronization.Data;
using Synchronization.Interface;

namespace Synchronization
{
    public class GhostMethodHandler
    {
        private readonly IGhost _Ghost;

        private readonly IGhostRequest _Requester;

        private readonly ReturnValueQueue _ReturnValueQueue;

        public GhostMethodHandler(IGhost ghost, ReturnValueQueue return_value_queue, IGhostRequest requester)
        {
            _Ghost = ghost;
            _ReturnValueQueue = return_value_queue;
            _Requester = requester;
        }

        public void Run(MethodInfo info, object[] args, IValue return_value)
        {
            var package = new PackageCallMethod
                              {
                                  EntityId = _Ghost.GetID(),
                                  MethodName = info.Name,
                                  MethodParams = args
                              };

            if(return_value != null)
            {
                package.ReturnId = _ReturnValueQueue.PushReturnValue(return_value);
            }

            _Requester.Request(ClientToServerOpCode.CallMethod, package);
        }
    }
}
