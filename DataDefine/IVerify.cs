using Library.Synchronize;

namespace DataDefine
{
    public interface IVerify
    {
        Value<bool> Login(string id, string password);
    }
}
