using System;
using System.Reflection;

namespace Gateway.Synchronize
{
    public delegate void CallMethodCallback(MethodInfo info, object[] args, IValue return_value);

    public interface IGhost
    {
        Guid GetID();

        object GetInstance();

        bool IsReturnType();

        Type GetType();

        event CallMethodCallback CallMethodEvent;
    }
}