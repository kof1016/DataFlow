using System;

namespace Synchronization
{
    public interface IEventProxyCreator
    {
        Delegate Create(Guid soul_id, int event_id, InvokeEventCallback invoke_event);

        Type GetType();

        string GetName();
    }
}
