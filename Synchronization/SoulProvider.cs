using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Library.Synchronize;

using Synchronization.Data;
using Synchronization.Interface;
using Synchronization.PreGenerated;

namespace Synchronization
{
    public class SoulProvider : IDisposable, ISoulBinder
    {
        private readonly Queue<object[]> _EventFilter = new Queue<object[]>();

        private readonly IRequestQueue _Peer;

        private readonly IResponseQueue _Queue;

        private readonly List<Soul> _Souls = new List<Soul>();

        private readonly Dictionary<Guid, IValue> _WaitValues = new Dictionary<Guid, IValue>();

        private DateTime _UpdatePropertyInterval;

        public SoulProvider(IRequestQueue peer, IResponseQueue queue)
        {
            _Queue = queue;
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

        private void _InvokeMethod(Guid entity_id, string method_name, Guid return_id, object[] args)
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

            var methodInfo =
                    (from m in soulInfo.MethodInfos
                     where m.Name == method_name && m.GetParameters().Length == args.Length
                     select m)
                    .FirstOrDefault();

            if(methodInfo == null)
            {
                return;
            }

            try
            {
                var returnValue = methodInfo.Invoke(soulInfo.ObjectInstance, args) as IValue;

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
                ReturnValue = value
            };
            _Queue.Push(ServerToClientOpCode.ReturnValue, package);
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

            _LoadSoul(newSoul.ObjectType, newSoul.ID, true);
            newSoul.ProcessDifferentValues(_UpdateProperty);
            _LoadSoulCompile(newSoul.ObjectType, newSoul.ID, return_id);
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
            _LoadSoul(newSoul.ObjectType, newSoul.ID, return_type);
            newSoul.ProcessDifferentValues(_UpdateProperty);
            _LoadSoulCompile(newSoul.ObjectType, newSoul.ID, return_id);
        }

        private Soul _NewSoul(object soul, Type soul_type)
        {
            var newSoul = new Soul
                              {
                                  ID = Guid.NewGuid(),
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
                newSoul.PropertyHandlers[i] = new Soul.PropertyHandler(property, property.Name);
            }

            _Souls.Add(newSoul);

            return newSoul;
        }

        private Delegate _BuildDelegate(EventInfo event_info, Guid new_soul_id, InvokeEventCallback invoke)
        {
            var raiseMethod = event_info.GetRaiseMethod();
            var parameterInfos = raiseMethod.GetParameters();
            var argTypes = parameterInfos.Select(p => p.ParameterType).ToArray();

            Type[] genericEventClosureTypes =
                {
                    typeof(GenericEventClosure),
                    typeof(GenericEventClosure<>),
                    typeof(GenericEventClosure<,>),
                    typeof(GenericEventClosure<,,>),
                    typeof(GenericEventClosure<,,,>),
                    typeof(GenericEventClosure<,,,,>)
                };

            var type = genericEventClosureTypes[argTypes.Length].MakeGenericType(argTypes);

            var instance = Activator.CreateInstance(type, new_soul_id, raiseMethod.Name, invoke);

            var actionType = type.GetMethod("GetDelegateType").Invoke(instance, new object[0]) as Type;

            return Delegate.CreateDelegate(actionType, instance, "Run", true);

            // Type[] genericEventClosureTypes =
            // {
            // typeof(Action),
            // typeof(GenericEventClosure<>),
            // typeof(GenericEventClosure<,>),
            // typeof(GenericEventClosure<,,>),
            // typeof(GenericEventClosure<,,,>),
            // typeof(GenericEventClosure<,,,,>)
            // };
        }

        private void _InvokeEvent(Guid entity_id, string event_name, object[] args)
        {
            var package = new PackageInvokeEvent
                              {
                                  EventName = event_name,
                                  EntityId = entity_id,
                                  EventParams = args
                              };

            lock(_EventFilter)
            {
                _EventFilter.Enqueue(package.EventParams);
            }
        }

        private void _LoadSoul(Type soul_type, Guid id, bool return_type)
        {
            var package = new PackageLoadSoul
                              {
                                  TypeName = soul_type.Name,
                                  EntityId = id,
                                  ReturnType = return_type
                              };
            _Queue.Push(ServerToClientOpCode.LoadSoul, package);
        }

        private void _UpdateProperty(Guid entity_id, string property_name, object val)
        {
            var package = new PackageUpdateProperty
                              {
                                  EntityId = entity_id,
                                  PropertyName = property_name,
                                  Arg = val
                              };

            _Queue.Push(ServerToClientOpCode.UpdateProperty, package);
        }

        private void _LoadSoulCompile(Type soul_type, Guid id, Guid return_id)
        {
            var package = new PackageLoadSoulCompile
                              {
                                  TypeName = soul_type.Name,
                                  EntityId = id,
                                  ReturnId = return_id
                              };

            _Queue.Push(ServerToClientOpCode.LoadSoulCompile, package);
        }

        private void _Unbind(object soul, Type type)
        {
            var soulInfo = (from soul_info in _Souls
                            where soul_info.ObjectType == type && ReferenceEquals(soul_info.ObjectInstance, soul)
                            select soul_info).SingleOrDefault();

            if(soulInfo != null)
            {
                foreach(var eventHandler in soulInfo.EventHandlers)
                {
                    eventHandler.EventInfo.RemoveEventHandler(soulInfo.ObjectInstance, eventHandler.DelegateObject);
                }

                _UnloadSoul(soulInfo.ObjectType, soulInfo.ID);

                _Souls.Remove(soulInfo);
            }
        }

        private void _UnloadSoul(Type soul_type, Guid id)
        {
            var package = new PackageUnloadSoul
                              {
                                  TypeName = soul_type.Name,
                                  EntityId = id
                              };
            _Queue.Push(ServerToClientOpCode.UnloadSoul, package);
        }      
    }
}