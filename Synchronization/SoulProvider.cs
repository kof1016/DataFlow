using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Linq;

using Library.Synchronize;

namespace Synchronization
{
    public class SoulProvider : IDisposable, ISoulBinder
    {
        private readonly Queue<object[]> _EventFilter = new Queue<object[]>();

        private readonly EventProvider _EventProvider;

        private readonly IRequestQueue _Peer;

        private readonly IResponseQueue _Queue;

        private readonly List<Soul> _Souls = new List<Soul>();

        private readonly Dictionary<Guid, IValue> _WaitValues = new Dictionary<Guid, IValue>();

        private DateTime _UpdatePropertyInterval;

        public SoulProvider(IRequestQueue peer, IResponseQueue queue)
        {
            _Queue = queue;
            _EventProvider;

            _Peer = peer;
            _Peer.OnInvokeMethodEvent += _InvokeMethod;
        }

        void IDisposable.Dispose()
        {
            _Peer.OnInvokeMethodEvent -= _InvokeMethod;
        }

        event Action ISoulBinder.OnBreakEvent
        {
            add
            {
                lock (_Peer)
                {
                    _Peer.OnBreakEvent += value;
                }
            }

            remove
            {
                lock (_Peer)
                {
                    _Peer.OnBreakEvent -= value;
                }
            }
        }

        void ISoulBinder.Return<TSoul>(TSoul soul)
        {
            if (soul == null)
            {
                throw new ArgumentNullException("soul");
            }

            _Bind(soul, true, Guid.Empty);
        }

        void ISoulBinder.Bind<TSoul>(TSoul soul)
        {
            if (soul == null)
            {
                throw new ArgumentNullException("soul");
            }

            _Bind(soul, false, Guid.Empty);
        }

        void ISoulBinder.Unbind<TSoul>(TSoul soul)
        {
            if (soul == null)
            {
                throw new ArgumentNullException("soul");
            }

            _Unbind(soul, typeof(TSoul));
        }

        private void _InvokeMethod(Guid entity_id, int method_id, Guid return_id, object[] args)
        {
            var soulInfo = (from soul in _Souls
                            where soul.ID == entity_id
                            select new
                                       {
                                           soul.MethodInfos,
                                           soul.ObjectInstance
                                       }).FirstOrDefault();
            if (soulInfo != null)
            {
                var info = method_id;
                var methodInfo =
                        (from m in soulInfo.MethodInfos where m == info && m.GetParameters().Count() == args.Count() select m)
                        .FirstOrDefault();
                if (methodInfo != null)
                {
                    try
                    {
                        var argObjects = args.Select(arg => _Serializer.Deserialize(arg));

                        var returnValue = methodInfo.Invoke(soulInfo.ObjectInstance, argObjects.ToArray());
                        if (returnValue != null)
                        {
                            _ReturnValue(return_id, returnValue as IValue);
                        }
                    }
                   catch (Exception e)
                    {
                        Log.Instance.WriteDebug(e.ToString());
                        _ErrorDeserialize(method_id.ToString(), return_id, e.Message);
                    }

                }
            }
        }

        private void _Bind<TSoul>(TSoul soul, bool return_type, Guid return_id)
        {
            var type = typeof(TSoul);

            var prevSoul = (from soulInfo in _Souls.UpdateSet()
                            where object.ReferenceEquals(soulInfo.ObjectInstance, soul) && soulInfo.ObjectType == typeof(TSoul)
                            select soulInfo).SingleOrDefault();

            if (prevSoul == null)
            {
                var new_soul = _NewSoul(soul, typeof(TSoul));

                _LoadSoul(new_soul.InterfaceId, new_soul.ID, return_type);
                new_soul.ProcessDiffentValues(_UpdateProperty);
                _LoadSoulCompile(new_soul.InterfaceId, new_soul.ID, return_id);
            }
        }

        private void _UpdateProperty(Guid entity_id, int property, object val)
        {


            var package = new PackageUpdateProperty();

            package.EntityId = entity_id;
            package.Property = property;


            package.Args = _Serializer.Serialize(val);

            _Queue.Push(ServerToClientOpCode.UpdateProperty, package.ToBuffer(_Serializer));
        }

        private void _Unbind(object soul, Type type)
        {
            var soulInfo = (from soul_info in _Souls.UpdateSet()
                            where object.ReferenceEquals(soul_info.ObjectInstance, soul) && soul_info.ObjectType == type
                            select soul_info).SingleOrDefault();

            // var soulInfo = _Souls.CreateInstnace((soul_info) => { return Object.ReferenceEquals(soul_info.ObjectInstance, soul) && soul_info.ObjectType == typeof(TSoul); });
            if (soulInfo != null)
            {
                foreach (var eventHandler in soulInfo.EventHandlers)
                {
                    eventHandler.EventInfo.RemoveEventHandler(soulInfo.ObjectInstance, eventHandler.DelegateObject);
                }

                _UnloadSoul(soulInfo.InterfaceId, soulInfo.ID);
                _Souls.Remove(s => { return s == soulInfo; });
            }
        }
    }
}
