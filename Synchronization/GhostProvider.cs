using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;

using Library.Synchronize;
using Library.TypeHelper;
using Library.Utility;

using Synchronization.Data;
using Synchronization.Interface;

namespace Synchronization
{
    public class GhostProvider
    {
        public event Action<string, string> OnErrorMethodEvent;

        private readonly AutoRelease _AutoRelease;

        private readonly GhostInterfaceProvider _GhostInterfaceProvider;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly Dictionary<string, IProvider> _ServerProviders;

        private readonly object _Sync = new object();

        private TimeCounter _PingTimeCounter = new TimeCounter();

        private Timer _PingTimer;

        private IGhostRequest _Requester;

        public long Ping { get; private set; }

        public bool Enable { get; private set; }

        public GhostProvider()
        {
            _GhostInterfaceProvider = new GhostInterfaceProvider();

            _ReturnValueQueue = new ReturnValueQueue();

            _ServerProviders = new Dictionary<string, IProvider>();
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

        public void OnResponse(ServerToClientOpCode id, object arg)
        {
            _OnResponse(id, arg);
            _AutoRelease.Update();
        }

        protected void _OnResponse(ServerToClientOpCode id, object arg)
        {
            switch(id)
            {
                case ServerToClientOpCode.Ping:
                    Ping = _PingTimeCounter.Ticks;
                    _StartPing();
                    break;
                case ServerToClientOpCode.UpdateProperty:
                    {
                        var data = arg as PackageUpdateProperty;

                        if(data == null)
                        {
                            throw new NullReferenceException("PackageUpdateProperty cast null");
                        }

                        _UpdateProperty(data.PropertyName, data.EntityId, data.Arg);
                    }

                    break;
                case ServerToClientOpCode.InvokeEvent:
                    {
                        var data = arg as PackageInvokeEvent;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageInvokeEvent cast null");
                        }

                        _InvokeEvent(data.EntityId, data.EventName, data.EventParams);
                    }

                    break;
                case ServerToClientOpCode.ErrorMethod:
                    {
                        var data = arg as PackageErrorMethod;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageErrorMethod cast null");
                        }

                        _ErrorReturnValue(data.ReturnTarget, data.Method, data.Message);
                    }

                    break;
                case ServerToClientOpCode.ReturnValue:
                    {
                        var data = arg as PackageReturnValue;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageReturnValue cast null");
                        }

                        _SetReturnValue(data.ReturnTarget, data.ReturnValue);
                    }

                    break;
                case ServerToClientOpCode.LoadSoulCompile:
                    {
                        var data = arg as PackageLoadSoulCompile;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageLoadSoulCompile cast null");
                        }

                        _LoadSoulCompile(data.TypeName, data.EntityId, data.ReturnId);
                    }

                    break;
                case ServerToClientOpCode.LoadSoul:
                    {
                        var data = arg as PackageLoadSoul;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageLoadSoul cast null");
                        }

                        _LoadSoul(data.TypeName, data.EntityId, data.ReturnType);
                    }

                    break;
                case ServerToClientOpCode.UnloadSoul:
                    {
                        var data = arg as PackageUnloadSoul;
                        if (data == null)
                        {
                            throw new NullReferenceException("PackageUnloadSoul cast null");
                        }

                        _UnloadSoul(data.TypeName, data.EntityId);
                    }

                    break;
                case ServerToClientOpCode.ProtocolSubmit:
                    {
                        var data = arg as PackageProtocolSubmit;

                        if (data == null)
                        {
                            throw new NullReferenceException("PackageProtocolSubmit cast null");
                        }
                        
                        // _ProtocolSubmit(data);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        private void _UpdateProperty(string property_name, Guid entity_id, object arg)
        {
            var ghostInterface = _FindGhost(entity_id);

            var instance = ghostInterface.GetInstance();

            var field = ghostInterface.GetType().GetField("_" + property_name, BindingFlags.Instance | BindingFlags.NonPublic);

            if(field != null)
            {
                field.SetValue(instance, arg);
            }
        }

        private void _ErrorReturnValue(Guid return_target, string method, string message)
        {
            _ReturnValueQueue.PopReturnValue(return_target);

            OnErrorMethodEvent?.Invoke(method, message);
        }

        private void _SetReturnValue(Guid return_target, object return_value)
        {
            var value = _ReturnValueQueue.PopReturnValue(return_target);
            value?.SetValue(return_value);
        }

        private void _LoadSoulCompile(string type_name, Guid entity_id, Guid return_id)
        {
            var provider = _QueryServerProvider(type_name);

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

        private void _LoadSoul(string type_name, Guid id, bool return_type)
        {
            var serverProvider = _QueryServerProvider(type_name);

            var type = _FindCurrentDomainType(type_name);

            var ghostInterface = _BuildGhostInstance(type, id, return_type);

            ghostInterface.CallMethodEvent += new GhostMethodHandler(ghostInterface, _ReturnValueQueue, _Requester).Run;

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

        private void _UnloadSoul(string type_name, Guid id)
        {
            var provider = _QueryServerProvider(type_name);

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
            return _QueryServerProvider(typeof(T).Name) as INotifier<T>;
        }

        private IProvider _QueryServerProvider(string type_name)
        {
            IProvider provider;
            lock(_ServerProviders)
            {
                if(_ServerProviders.TryGetValue(type_name, out provider) == false)
                {
                    provider = _BuildServerProvider(type_name);
                    _ServerProviders.Add(type_name, provider);
                }
            }

            return provider;
        }

        private IProvider _BuildServerProvider(string type_name)
        {
            var providerTemplateType = typeof(TProvider<>);

            var type = _FindCurrentDomainType(type_name);

            var providerType = providerTemplateType.MakeGenericType(type);

            return Activator.CreateInstance(providerType) as IProvider;
        }

        private void _InvokeEvent(Guid ghost_id, string event_name, object[] event_params)
        {
            var ghost = _FindGhost(ghost_id);

            var type = ghost.GetType();

            var instance = ghost.GetInstance();

            var eventInfos = type.GetField("_" + event_name, BindingFlags.Instance | BindingFlags.NonPublic);

            var fieldValue = eventInfos?.GetValue(instance);

            var fieldValueDelegate = fieldValue as Delegate;

            try
            {
                fieldValueDelegate?.DynamicInvoke(event_params);
            }
            catch(TargetInvocationException tie)
            {
                throw new Exception($"Call event error in {type.FullName}:{event_name}. \n{tie.InnerException}");
            }
            catch(Exception)
            {
                throw new Exception($"Call event error in {type.FullName}:{event_name}.");
            }
        }

        private Type _QueryGhostType(Type ghost_base_type)
        {
            if(_GhostInterfaceProvider.Find(ghost_base_type) != null)
            {
                return _GhostInterfaceProvider.Find(ghost_base_type);
            }

            var ghostType = new AssemblyBuilder().Build(ghost_base_type, typeof(IGhost).Assembly.Location, typeof(IEventProxyCreator).Assembly.Location);

            _GhostInterfaceProvider.Add(ghost_base_type, ghostType);

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
