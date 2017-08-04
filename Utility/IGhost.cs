using System;
using System.Reflection;

namespace Utility
{
    public delegate void CallMethodCallback(MethodInfo info, object[] args, IValue return_value);

    public interface IGhost
    {
        Guid GetID();

        object GetInstance();

        bool IsReturnType();

        event CallMethodCallback CallMethodEvent;
    }
}