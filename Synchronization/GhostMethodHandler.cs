using System.Linq;
using System.Reflection;

using Gateway.Synchronize;

using Synchronization.Data;
using Synchronization.Interface;
using Synchronization.PreGenerated;

namespace Synchronization
{
    public class GhostMethodHandler
    {
        private readonly IGhost _Ghost;

        private readonly IGhostRequest _Requester;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly IProtocol _Protocol;

        public GhostMethodHandler(IGhost ghost, ReturnValueQueue return_value_queue, IProtocol protocol, IGhostRequest requester)
        {
            _Ghost = ghost;
            _ReturnValueQueue = return_value_queue;
            _Protocol = protocol;
            _Requester = requester;
        }

        public void Run(MethodInfo info, object[] args, IValue return_value)
        {
            var map = _Protocol.GetMemberMap();
            var serialize = _Protocol.GetSerialize();
            var methodId = map.GetMethod(info);

            var package = new PackageCallMethod
            {
                EntityId = _Ghost.GetID(),
                MethodId = methodId,
                MethodParams = args.Select(arg => serialize.Serialize(arg)).ToArray()
            };

            if(return_value != null)
            {
                package.ReturnId = _ReturnValueQueue.PushReturnValue(return_value);
            }

            _Requester.Request(ClientToServerOpCode.CallMethod, serialize.Serialize(package));
        }
    }
}
