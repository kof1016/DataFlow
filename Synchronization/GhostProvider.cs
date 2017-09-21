using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;

using Library.Serialization;
using Library.Synchronize;
using Library.Utility;

using Synchronization.Data;
using Synchronization.Interface;
using Synchronization.PreGenerated;

namespace Synchronization
{
    public class GhostProvider
    {
        public event Action<string, string> OnErrorMethodEvent;

        private readonly AutoRelease _AutoRelease;

        private readonly InterfaceProvider _GhostInterfaceProvider;

        private readonly IProtocol _Protocol;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly ISerializer _Serializer;

        private readonly Dictionary<Type, IProvider> _ServerProviders;

        private readonly object _Sync = new object();

        private TimeCounter _PingTimeCounter = new TimeCounter();

        private Timer _PingTimer;

        private IGhostRequest _Requester;

        public long Ping { get; private set; }

        public bool Enable { get; private set; }

        public GhostProvider(IProtocol protocol)
        {
            _Protocol = protocol;
            _Serializer = _Protocol.GetSerialize();
            _GhostInterfaceProvider = protocol.GetInterfaceProvider();

            _ReturnValueQueue = new ReturnValueQueue();

            _ServerProviders = new Dictionary<Type, IProvider>();

            _AutoRelease = new AutoRelease(_Requester);
        }

        public void Initial(IGhostRequest req)
        {
            _Requester = req;
            _StartPing();

            Enable = true;
        }

        public void Finial()
        {
            Enable = false;
            lock(_ServerProviders)
            {
                foreach(var providerPair in _ServerProviders)
                {
                    providerPair.Value.ClearGhosts();
                }
            }

            _EndPing();
        }

        public void OnResponse(ServerToClientOpCode id, byte[] args)
        {
            _OnResponse(id, args);
            _AutoRelease.Update();
        }

        protected void _OnResponse(ServerToClientOpCode id, byte[] args)
        {
            switch(id)
            {
                case ServerToClientOpCode.Ping:
                    Ping = _PingTimeCounter.Ticks;
                    _StartPing();
                    break;
                case ServerToClientOpCode.UpdateProperty:
                {
                    var data = args.ToPackageData<PackageUpdateProperty>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageUpdateProperty cast null");
                    }

                    _UpdateProperty(data.PropertyId, data.EntityId, data.Args);
                }

                    break;
                case ServerToClientOpCode.InvokeEvent:
                {
                    var data = args.ToPackageData<PackageInvokeEvent>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageInvokeEvent cast null");
                    }

                    _InvokeEvent(data.EntityId, data.EventId, data.EventParams);
                }

                    break;
                case ServerToClientOpCode.ErrorMethod:
                {
                    var data = args.ToPackageData<PackageErrorMethod>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageErrorMethod cast null");
                    }

                    _ErrorReturnValue(data.ReturnTarget, data.Method, data.Message);
                }

                    break;
                case ServerToClientOpCode.ReturnValue:
                {
                    var data = args.ToPackageData<PackageReturnValue>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageReturnValue cast null");
                    }

                    _SetReturnValue(data.ReturnTarget, data.ReturnValue);
                }

                    break;
                case ServerToClientOpCode.LoadSoulCompile:
                {
                    var data = args.ToPackageData<PackageLoadSoulCompile>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageLoadSoulCompile cast null");
                    }

                    _LoadSoulCompile(data.TypeId, data.EntityId, data.ReturnId);
                }

                    break;
                case ServerToClientOpCode.LoadSoul:
                {
                    var data = args.ToPackageData<PackageLoadSoul>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageLoadSoul cast null");
                    }

                    _LoadSoul(data.TypeId, data.EntityId, data.ReturnType);
                }

                    break;

                case ServerToClientOpCode.UnloadSoul:
                {
                    var data = args.ToPackageData<PackageUnloadSoul>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageUnloadSoul cast null");
                    }

                    _UnloadSoul(data.TypeId, data.EntityId);
                }

                    break;
                case ServerToClientOpCode.ProtocolSubmit:
                {
                    var data = args.ToPackageData<PackageProtocolSubmit>(_Serializer);

                    if(data == null)
                    {
                        throw new NullReferenceException("PackageProtocolSubmit cast null");
                    }
                }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        private void _UpdateProperty(int property, Guid entity_id, byte[] arg)
        {
            var ghost = _FindGhost(entity_id);

            if (ghost != null)
            {
                var map = _Protocol.GetMemberMap();
                var info = map.GetProperty(property);
                var value = _Serializer.Deserialize(arg);
                var instance = ghost.GetInstance();
                var type = _GhostInterfaceProvider.Find(info.DeclaringType);
                var field = type.GetField("_" + info.Name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(instance, value);
                }

            }
        }

        private void _ErrorReturnValue(Guid return_target, string method, string message)
        {
            _ReturnValueQueue.PopReturnValue(return_target);

            OnErrorMethodEvent?.Invoke(method, message);
        }

        private void _SetReturnValue(Guid return_target, byte[] return_value)
        {
            var value = _ReturnValueQueue.PopReturnValue(return_target);
            value?.SetValue(_Serializer.Deserialize(return_value));
        }

        private void _LoadSoulCompile(int type_id, Guid entity_id, Guid return_id)
        {
            var map = _Protocol.GetMemberMap();
            var type = map.GetInterface(type_id);

            var provider = _QueryServerProvider(type);

            if(provider != null)
            {
                var ghost = provider.Ready(entity_id);
                _SetReturnValue(return_id, ghost);
            }
        }

        private void _SetReturnValue(Guid return_id, IGhost ghost)
        {
            var value = _ReturnValueQueue.PopReturnValue(return_id);
            value?.SetValue(ghost);
        }

        private void _LoadSoul(int type_id, Guid id, bool return_type)
        {
            var map = _Protocol.GetMemberMap();
            var type = map.GetInterface(type_id);

            var serverProvider = _QueryServerProvider(type);

            var ghostInterface = _BuildGhostInstance(type, id, return_type);

            ghostInterface.CallMethodEvent += new GhostMethodHandler(ghostInterface, _ReturnValueQueue, _Protocol, _Requester).Run;

            serverProvider.Add(ghostInterface);

            if(ghostInterface.IsReturnType())
            {
                _RegisterRelease(ghostInterface);
            }
        }

        private void _RegisterRelease(IGhost ghost)
        {
            _AutoRelease.Register(ghost);
        }

        private Type _FindCurrentDomainType(string type_name)
        {
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(var t in asm.GetTypes())
                {
                    if(t.Name == type_name)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        private void _UnloadSoul(int type_id, Guid id)
        {
            var map = _Protocol.GetMemberMap();
            var type = map.GetInterface(type_id);

            var provider = _QueryServerProvider(type);

            provider?.Remove(id);
        }

        private IGhost _BuildGhostInstance(Type ghost_base_type, Guid id, bool return_type)
        {
            if(ghost_base_type == null)
            {
                throw new ArgumentNullException($"ghost_base_type null plz check dll loader !!");
            }

            var ghostType = _QueryGhostType(ghost_base_type);

            var constructor = ghostType.GetConstructor(
                                                       new[]
                                                       {
                                                           typeof(Guid),
                                                           typeof(Type),
                                                           typeof(bool)
                                                       });
            if(constructor == null)
            {
                var constructorInfos = new List<string>();

                foreach(var constructorInfo in ghostType.GetConstructors())
                {
                    constructorInfos.Add("(" + constructorInfo.GetParameters() + ")");
                }

                throw new Exception($"{ghostType.FullName} Not found constructor.\n{string.Join("\n", constructorInfos.ToArray())}");
            }

            var o = constructor.Invoke(
                                       new object[]
                                       {
                                           id,
                                           ghostType,
                                           return_type
                                       });

            return (IGhost)o;
        }

        public INotifier<T> QueryProvider<T>()
        {
            return _QueryServerProvider(typeof(T)) as INotifier<T>;
        }

        private IProvider _QueryServerProvider(Type type)
        {
            IProvider provider;
            lock(_ServerProviders)
            {
                if(_ServerProviders.TryGetValue(type, out provider))
                {
                    return provider;
                }

                provider = _BuildServerProvider(type);
                _ServerProviders.Add(type, provider);
            }

            return provider;
        }

        private IProvider _BuildServerProvider(Type type)
        {
            var providerTemplateType = typeof(TProvider<>);

            var providerType = providerTemplateType.MakeGenericType(type);

            return Activator.CreateInstance(providerType) as IProvider;
        }

        private void _InvokeEvent(Guid ghost_id, int event_id, byte[][] event_params)
        {
            var ghost = _FindGhost(ghost_id);

            if (ghost != null)
            {
                var map = _Protocol.GetMemberMap();
                var info = map.GetEvent(event_id);


                Type type = _GhostInterfaceProvider.Find(info.DeclaringType);
                var instance = ghost.GetInstance();
                if (type != null)
                {
                    var eventInfos = type.GetField("_" + info.Name, BindingFlags.Instance | BindingFlags.NonPublic);
                    var fieldValue = eventInfos.GetValue(instance);
                    if (fieldValue is Delegate)
                    {
                        var fieldValueDelegate = fieldValue as Delegate;

                        var pars = (from a in event_params select _Serializer.Deserialize(a)).ToArray();
                        try
                        {
                            fieldValueDelegate.DynamicInvoke(pars);
                        }
                        catch (TargetInvocationException tie)
                        {
                            
                            throw tie;
                        }
                        catch (Exception e)
                        {
                            
                            throw e;
                        }
                    }
                }
            }
        }

        private Type _QueryGhostType(Type ghost_base_type)
        {
            // if(_GhostInterfaceProvider.Find(ghost_base_type) != null)
            // {
            // return _GhostInterfaceProvider.Find(ghost_base_type);
            // }
            // var ghostType = new AssemblyBuilder().Build(ghost_base_type, typeof(IGhost).Assembly.Location, typeof(IEventProxyCreator).Assembly.Location);
            // _GhostInterfaceProvider.Add(ghost_base_type, ghostType);
            // return _GhostInterfaceProvider.Find(ghost_base_type);
            return _GhostInterfaceProvider.Find(ghost_base_type);
        }

        private IGhost _FindGhost(Guid ghost_id)
        {
            lock(_ServerProviders)
            {
                return (from provider in _ServerProviders
                        let r = (from g in provider.Value.Ghosts
                                 where ghost_id == g.GetID()
                                 select g).FirstOrDefault()
                        where r != null
                        select r).FirstOrDefault();
            }
        }

        protected void _StartPing()
        {
            _EndPing();
            lock(_Sync)
            {
                _PingTimer = new Timer(1000)
                {
                    Enabled = true,
                    AutoReset = true
                };
                _PingTimer.Elapsed += _PingTimerElapsed;
                _PingTimer.Start();
            }
        }

        private void _PingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock(_Sync)
            {
                if(_PingTimer != null)
                {
                    _PingTimeCounter = new TimeCounter();
                    _Requester.Request(ClientToServerOpCode.Ping, new byte[0]);
                }
            }

            _EndPing();
        }

        protected void _EndPing()
        {
            lock(_Sync)
            {
                if(_PingTimer == null)
                {
                    return;
                }

                _PingTimer.Stop();
                _PingTimer = null;
            }
        }
    }
}
