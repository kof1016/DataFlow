using Gateway.Synchronize;

namespace DataDefine
{
    public interface IVerify
    {
        int TestProperty { get; }

        event System.Action<bool> TestEvent;

        Value<bool> Login(string id, string password);
    }
}
