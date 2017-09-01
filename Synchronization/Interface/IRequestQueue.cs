using System;

namespace Synchronization
{
    public delegate void InvokeMethodCallback(Guid entity_id, string method_name, Guid return_id, object[] args);

    public interface IRequestQueue
    {
        event Action OnBreakEvent;

        event InvokeMethodCallback OnInvokeMethodEvent;

        void Update();
    }
}
