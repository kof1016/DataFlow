
using Gateway.Serialization;

namespace Synchronization.PreGenerated
{
    public interface IProtocol
    {
        EventProvider GetEventProvider();

        InterfaceProvider GetInterfaceProvider();

        ISerializer GetSerialize();

        MemberMap GetMemberMap();

        byte[] VerificationCode { get; }
    }
}