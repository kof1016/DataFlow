using System;

namespace Synchronization
{
    public interface IResponseQueue
    {
        void Push(ServerToClientOpCode code, Type package);
    }
}