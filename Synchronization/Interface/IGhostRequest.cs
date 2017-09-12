using Synchronization.Data;

namespace Synchronization.Interface
{
    public interface IGhostRequest
    {
        void Request(ClientToServerOpCode code, object arg);
    }
}