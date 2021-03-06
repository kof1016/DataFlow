using System;

namespace Synchronization.PreGenerated
{
    public delegate void InvokeEventCallback(Guid entity_id, int event_id, object[] args);

    public class GenericEventClosure
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action);
        }

        public void Run()
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                             });
        }
    }

    public class GenericEventClosure<T1>
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action<T1>);
        }

        public void Run(T1 arg1)
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                                 arg1
                             });
        }
    }

    public class GenericEventClosure<T1, T2>
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action<T1, T2>);
        }

        public void Run(T1 arg1, T2 arg2)
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                                 arg1,
                                 arg2
                             });
        }
    }

    public class GenericEventClosure<T1, T2, T3>
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action<T1, T2, T3>);
        }

        public void Run(T1 arg1, T2 arg2, T3 arg3)
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                                 arg1,
                                 arg2,
                                 arg3
                             });
        }
    }

    public class GenericEventClosure<T1, T2, T3, T4>
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action<T1, T2, T3, T4>);
        }

        public void Run(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            InvokeEvent(
                        _EntityId,
                        _EventId,
                        new object[]
                            {
                                arg1,
                                arg2,
                                arg3,
                                arg4
                            });
        }
    }

    public class GenericEventClosure<T1, T2, T3, T4, T5>
    {
        private delegate void Action5(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action5);
        }

        public void Run(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                                 arg1,
                                 arg2,
                                 arg3,
                                 arg4,
                                 arg5
                             });
        }
    }
}
