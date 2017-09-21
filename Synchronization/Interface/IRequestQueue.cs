using System;

namespace Synchronization.Interface
{
    public delegate void InvokeMethodCallback(Guid entity_id, int method_id, Guid return_id, byte[][] args);

    public interface IRequestQueue
    {
        event Action OnBreakEvent;

        event InvokeMethodCallback OnInvokeMethodEvent;

        void Update();
    }
}
