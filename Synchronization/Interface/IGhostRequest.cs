namespace Synchronization
{
    public interface IGhostRequest
    {
        void Request(ClientToServerOpCode code, object arg);
    }
}