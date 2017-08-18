using System;
using System.Diagnostics.Eventing;

namespace Synchronization
{
    public interface IProtocol
    {
        EventProvider GetEventProvider();

        InterfaceProvider GetInterfaceProvider();

        MemberMap GetMemberMap();

        byte[] VerificationCode { get; }
    }
}