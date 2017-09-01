using Library.Synchronize;

namespace DataDefine
{
    public interface IVerify2
    {
        Value<bool> Login(string id, string password);
    }
    public interface IVerify
    {
        Value<bool> Login(string id, string password);
    }
}
