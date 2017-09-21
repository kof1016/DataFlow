using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Library.Serialization;
using Library.Synchronize;

using Synchronization.Data;
using Synchronization.Interface;
using Synchronization.PreGenerated;

namespace Synchronization
{
    public class SoulProvider : IDisposable, ISoulBinder
    {
        private readonly Queue<byte[]> _EventFilter = new Queue<byte[]>();

        private readonly IRequestQueue _Peer;

        private readonly IResponseQueue _Queue;

        private readonly IProtocol _Protocol;

        private readonly List<Soul> _Souls = new List<Soul>();

        private readonly Dictionary<Guid, IValue> _WaitValues = new Dictionary<Guid, IValue>();

        private DateTime _UpdatePropertyInterval;

        private readonly ISerializer _Serialize;

        public SoulProvider(IRequestQueue peer, IResponseQueue queue, IProtocol protocol)
        {
            _Queue = queue;
            _Protocol = protocol;
            _Serialize = protocol.GetSerialize();
            _Peer = peer;
            _Peer.OnInvokeMethodEvent += _InvokeMethod;
        }

        public void Update()
        {
            var intervalSpan = DateTime.Now - _UpdatePropertyInterval;
            var intervalSeconds = intervalSpan.TotalSeconds;
            if (intervalSeconds > 0.5)
            {
                foreach (var soul in _Souls)
                {
                    soul.ProcessDifferentValues(_UpdateProperty);
                }

                _UpdatePropertyInterval = DateTime.Now;
            }

            lock (_EventFilter)
            {
                foreach (var filter in _EventFilter)
                {
                    _Queue.Push(ServerToClientOpCode.InvokeEvent, filter);
                }

                _EventFilter.Clear();
            }
        }

        public void Unbind(Guid entity_id)
        {
            var soul = (from s in _Souls
                        where s.ID == entity_id
                        select s).FirstOrDefault();

            if (soul != null)
            {
                _Unbind(soul.ObjectInstance, soul.ObjectType);
            }
        }

        void IDisposable.Dispose()
        {
            _Peer.OnInvokeMethodEvent -= _InvokeMethod;
        }

        event Action ISoulBinder.OnBreakEvent
        {
            add
            {
                lock(_Peer)
                {
                    _Peer.OnBreakEvent += value;
                }
            }

            remove
            {
                lock(_Peer)
                {
                    _Peer.OnBreakEvent -= value;
                }
            }
        }

        void ISoulBinder.Return<TSoul>(TSoul soul)
        {
            if(soul == null)
            {
                throw new ArgumentNullException("soul");
            }

            _Bind(soul, true, Guid.Empty);
        }

        void ISoulBinder.Bind<TSoul>(TSoul soul)
        {
            if(soul == null)
            {
                throw new ArgumentNullException("soul");
            }

            _Bind(soul, false, Guid.Empty);
        }

        void ISoulBinder.Unbind<TSoul>(TSoul soul)
        {
            if(soul == null)
            {
                throw new ArgumentNullException("unbind erro is soul null");
            }

            _Unbind(soul, typeof(TSoul));
        }

        private void _InvokeMethod(Guid entity_id, int method_id, Guid return_id, byte[][] args)
        {
            var soulInfo = (from soul in _Souls
                            where soul.ID == entity_id
                            select new
                                       {
                                           soul.MethodInfos,
                                           soul.ObjectInstance
                                       }).FirstOrDefault();
            if(soulInfo == null)
            {
                return;
            }

            var info = _Protocol.GetMemberMap().GetMethod(method_id);
            
                var methodInfo =
                    (from m in soulInfo.MethodInfos
                     where 
                     m == _Protocol.GetMemberMap().GetMethod(method_id) 
                     && m.GetParameters().Length == args.Length
                     select m)
                    .FirstOrDefault();

            if(methodInfo == null)
            {
                return;
            }

            try
            {
                var argObjects = args.Select(arg => _Serialize.Deserialize(arg));

                var returnValue = methodInfo.Invoke(soulInfo.ObjectInstance, argObjects.ToArray()) as IValue;

                if(returnValue != null)
                {
                    _ReturnValue(return_id, returnValue);
                }
            }
            catch(Exception)
            {
                throw new ArgumentNullException($"return value error {return_id}");
            }
        }

        private void _ReturnValue(Guid return_id, IValue return_value)
        {
            if(_WaitValues.TryGetValue(return_id, out IValue outValue))
            {
                return;
            }

            _WaitValues.Add(return_id, return_value);
            return_value.QueryValue(
                obj =>
                {
                    if(return_value.IsInterface() == false)
                    {
                        _ReturnDataValue(return_id, return_value);
                    }
                    else
                    {
                        _ReturnSoulValue(return_id, return_value);
                    }

                    _WaitValues.Remove(return_id);
                });
        }

        private void _ReturnDataValue(Guid return_id, IValue return_value)
        {
            var value = return_value.GetObject();
            var package = new PackageReturnValue()
            {
                ReturnTarget = return_id,
                ReturnValue = _Serialize.Serialize(value)
            };
            _Queue.Push(ServerToClientOpCode.ReturnValue, _Serialize.Serialize(package));
        }

        private void _ReturnSoulValue(Guid return_id, IValue return_value)
        {
            var soul = return_value.GetObject();
            var type = return_value.GetObjectType();

            var prevSoul = (from soulInfo in _Souls
                            where soulInfo.ObjectType == type && ReferenceEquals(soulInfo.ObjectInstance, soul) 
                            select soulInfo).SingleOrDefault();

            if(prevSoul != null)
            {
                return;
            }

            var newSoul = _NewSoul(soul, type);

            _LoadSoul(newSoul.InterfaceId, newSoul.ID, true);
            newSoul.ProcessDifferentValues(_UpdateProperty);
            _LoadSoulCompile(newSoul.InterfaceId, newSoul.ID, return_id);
        }

        private void _Bind<TSoul>(TSoul soul, bool return_type, Guid return_id)
        {
            var prevSoul = (from soulInfo in _Souls
                            where soulInfo.ObjectType == typeof(TSoul) && ReferenceEquals(soulInfo.ObjectInstance, soul)
                            select soulInfo).SingleOrDefault();

            if(prevSoul != null)
            {
                return;
            }

            var newSoul = _NewSoul(soul, typeof(TSoul));
            _LoadSoul(newSoul.InterfaceId, newSoul.ID, return_type);
            newSoul.ProcessDifferentValues(_UpdateProperty);
            _LoadSoulCompile(newSoul.InterfaceId, newSoul.ID, return_id);
        }

        private Soul _NewSoul(object soul, Type soul_type)
        {
            var map = _Protocol.GetMemberMap();
            var interfaceId = map.GetInterface(soul_type);

            var newSoul = new Soul
                              {
                                  ID = Guid.NewGuid(),
                                  InterfaceId = interfaceId,
                                  ObjectType = soul_type,
                                  ObjectInstance = soul,
                                  MethodInfos = soul_type.GetMethods()
                              };

            var eventInfos = soul_type.GetEvents();
            newSoul.EventHandlers = new List<Soul.EventHandler>();

            foreach(var eventInfo in eventInfos)
            {
                try
                {
                    var buildDelegate = _BuildDelegate(eventInfo, newSoul.ID, _InvokeEvent);

                    var eventHandler = new Soul.EventHandler
                                           {
                                               EventInfo = eventInfo,
                                               DelegateObject = buildDelegate
                                           };

                    newSoul.EventHandlers.Add(eventHandler);

                    var addMethod = eventInfo.GetAddMethod();
                    addMethod.Invoke(soul, new object[] { buildDelegate });
                }
                catch(Exception ex)
                {
                    throw new Exception("new soul error" + ex);
                }
            }

            // property 
            var properties = soul_type.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            newSoul.PropertyHandlers = new Soul.PropertyHandler[properties.Length];
            for(var i = 0; i < properties.Length; ++i)
            {
                var property = properties[i];
                var id = map.GetProperty(property);
                newSoul.PropertyHandlers[i] = new Soul.PropertyHandler(property, id);
            }

            _Souls.Add(newSoul);

            return newSoul;
        }

        private Delegate _BuildDelegate(EventInfo event_info, Guid new_soul_id, InvokeEventCallback invoke)
        {


            var eventCreator = _Protocol.GetEventProvider().Find(event_info);
            var map = _Protocol.GetMemberMap();
            var id = map.GetEvent(event_info);
            return eventCreator.Create(new_soul_id, id, invoke);
        }

        private void _InvokeEvent(Guid entity_id, int event_id, object[] args)
        {
            var package = new PackageInvokeEvent
                              {
                                  EventId = event_id,
                                  EntityId = entity_id,
                                  EventParams = (from a in args select _Serialize.Serialize(a)).ToArray()
                                };

            lock(_EventFilter)
            {
                _EventFilter.Enqueue(_Serialize.Serialize(package));
            }
        }

        private void _LoadSoul(int soul_type_id, Guid id, bool return_type)
        {
            var package = new PackageLoadSoul
                              {
                                  TypeId = soul_type_id,
                                  EntityId = id,
                                  ReturnType = return_type
                              };
            _Queue.Push(ServerToClientOpCode.LoadSoul, _Serialize.Serialize(package));
        }

        private void _UpdateProperty(Guid entity_id, int property_id, object val)
        {
            var package = new PackageUpdateProperty
                              {
                                  EntityId = entity_id,
                                  PropertyId = property_id,
                                  Args = _Serialize.Serialize(val)
                              };

            _Queue.Push(ServerToClientOpCode.UpdateProperty, _Serialize.Serialize(package));
        }

        private void _LoadSoulCompile(int soul_type_id, Guid id, Guid return_id)
        {
            var package = new PackageLoadSoulCompile
                              {
                                  TypeId = soul_type_id,
                                  EntityId = id,
                                  ReturnId = return_id
                              };

            _Queue.Push(ServerToClientOpCode.LoadSoulCompile, _Serialize.Serialize(package));
        }

        private void _Unbind(object soul, Type type)
        {
            var soulInfo = (from soul_info in _Souls
                            where soul_info.ObjectType == type && ReferenceEquals(soul_info.ObjectInstance, soul)
                            select soul_info).SingleOrDefault();

            if(soulInfo == null)
            {
                return;
            }

            foreach(var eventHandler in soulInfo.EventHandlers)
            {
                eventHandler.EventInfo.RemoveEventHandler(soulInfo.ObjectInstance, eventHandler.DelegateObject);
            }

            _UnloadSoul(soulInfo.InterfaceId, soulInfo.ID);

            _Souls.Remove(soulInfo);
        }

        private void _UnloadSoul(int soul_type_id, Guid id)
        {
            var package = new PackageUnloadSoul
                              {
                                  TypeId = soul_type_id,
                                  EntityId = id
                              };
            _Queue.Push(ServerToClientOpCode.UnloadSoul, _Serialize.Serialize(package));
        }      
    }
}