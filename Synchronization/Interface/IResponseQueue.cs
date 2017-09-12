using Synchronization.Data;

namespace Synchronization.Interface
{
    public interface IResponseQueue
    {
        void Push(ServerToClientOpCode code, object package);
    }
}