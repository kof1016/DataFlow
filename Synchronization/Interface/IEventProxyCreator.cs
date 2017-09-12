using System;

using Synchronization.PreGenerated;

namespace Synchronization.Interface
{
    public interface IEventProxyCreator
    {
        Delegate Create(Guid soul_id, int event_id, InvokeEventCallback invoke_event);

        Type GetType();

        string GetName();
    }
}
