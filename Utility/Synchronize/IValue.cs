using System;

namespace Gateway.Synchronize
{
    public interface IValue
    {
        object GetObject();

        void SetValue(object val);

        void QueryValue(Action<object> action);

        void SetValue(IGhost ghost);

        bool IsInterface();

        Type GetObjectType();

    }
}