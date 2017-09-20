
namespace Synchronization.PreGenerated
{
    public interface IProtocol
    {
        EventProvider GetEventProvider();

        InterfaceProvider GetInterfaceProvider();

        MemberMap GetMemberMap();

        byte[] VerificationCode { get; }
    }
}