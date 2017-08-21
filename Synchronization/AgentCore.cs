using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;

using Library.Synchronize;
using Library.TypeHelper;
using Library.Utility;

namespace Synchronization
{
    public class AgentCore
    {
        public event Action<byte[], byte[]> OnErrorVerifyEvent;

        public event Action<string, string> OnErrorMethodEvent;

        private readonly AutoRelease _AutoRelease;

        private readonly GhostInterfaceProvider _GhostProvider;

        private readonly Dictionary<string, IProvider> _ServerProviders;

        private readonly ReturnValueQueue _ReturnValueQueue;

        private readonly object _Sync = new object();

        private MemberMap _MemberMap;

        private TimeCounter _PingTimeCounter = new TimeCounter();

        private Timer _PingTimer;

        private IGhostRequest _Requester;

        public long Ping { get; private set; }

        public bool Enable { get; private set; }

        public AgentCore()
        {
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
                case ServerToClientOpCode.PING:
                    Ping = _PingTimeCounter.Ticks;
                    _StartPing();
                    break;
                case ServerToClientOpCode.UpdateProperty:
                    {
                        var data = arg as PackageUpdateProperty;
                        _UpdateProperty(data.EntityId, data.Property, data.Args);
                    }

                    break;
                case ServerToClientOpCode.InvokeEvent:
                    {
                        var data = arg as PackageInvokeEvent;
                        _InvokeEvent(data.EntityId, data.Event, data.EventParams);
                    }

                    break;
                case ServerToClientOpCode.ErrorMethod:
                    {
                        var data = arg as PackageErrorMethod;
                        _ErrorReturnValue(data.ReturnTarget, data.Method, data.Message);
                    }

                    break;
                case ServerToClientOpCode.ReturnValue:
                    {
                        var data = arg as PackageReturnValue;
                        _SetReturnValue(data.ReturnTarget, data.ReturnValue);
                    }

                    break;
                case ServerToClientOpCode.LoadSoulCompile:
                    {
                        var data = arg as PackageLoadSoulCompile;
                        _LoadSoulCompile(data.TypeId, data.EntityId, data.ReturnId);
                    }

                    break;
                case ServerToClientOpCode.LoadSoul:
                    {
                        var data = arg as PackageLoadSoul;
                        _LoadSoul(data.TypeName, data.EntityId, data.ReturnType);
                    }

                    break;
                case ServerToClientOpCode.UnloadSoul:
                    {
                        var data = arg as PackageUnloadSoul;
                        _UnloadSoul(data.TypeId, data.EntityId);
                    }

                    break;
                case ServerToClientOpCode.ProtocolSubmit:
                    {
                        var data = arg as PackageProtocolSubmit;
                        _ProtocolSubmit(data);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        private void _UpdateProperty(Guid entity_id, int property, object[] args)
        {
            var ghost = _FindGhost(entity_id);
            if(ghost == null)
            {
                return;
            }

            var info = _MemberMap.GetProperty(property);
            var instance = ghost.GetInstance();
            var type = _GhostProvider.Find(info.DeclaringType);
            var field = type.GetField("_" + info.Name, BindingFlags.Instance | BindingFlags.NonPublic);
            if(field != null)
            {
                field.SetValue(instance, args);
            }
        }

        private void _ErrorReturnValue(Guid return_target, string method, string message)
        {
            _ReturnValueQueue.PopReturnValue(return_target);

            OnErrorMethodEvent?.Invoke(method, message);
        }

        private void _SetReturnValue(Guid return_target, object[] return_value)
        {
            var value = _ReturnValueQueue.PopReturnValue(return_target);
            value?.SetValue(return_value);
        }

        private void _LoadSoulCompile(int type_id, Guid entity_id, Guid return_id)
        {
            var map = _MemberMap;

            var type = map.GetInterface(type_id);

            var provider = _QueryProvider(type);
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

        CodeBuilder codeBuilder = new CodeBuilder();

        private void _LoadSoul(string type_name, Guid id, bool return_type)
        {
            Type type;

            var provider = _QueryProvider(type_name);

            var ghost = _BuildGhost(_FindType(type_name), _Requester, id, return_type);


//            lock (_ServerProviders)
//            {
//                if(_ServerProviders.ContainsKey(type_name)) // find soul typename
//                {
//                
//                }
//                else // create ghost type
//                {
//                    codeBuilder.GpiEvent += (s, s1) => { };
//
//                    codeBuilder.Build(t);
//
//                    // call proxy  
//                    _FindType(type_name);
//                }
//            }
            

           // var provider = _QueryProvider(type);
            //var ghost = _BuildGhost(type, _Requester, id, return_type);

            ghost.CallMethodEvent += new GhostMethodHandler(ghost, _ReturnValueQueue, _Requester).Run;

            provider.Add(ghost);

            if(ghost.IsReturnType())
            {
                _RegisterRelease(ghost);
            }
        }

        private Type _FindType(string type_name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.Name == type_name)
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
            var provider = _QueryProvider(type);
            if(provider != null)
            {
                provider.Remove(id);
            }
        }

        private IGhost _BuildGhost(Type ghost_base_type, IGhostRequest peer, Guid id, bool return_type)
        {
            if (peer == null)
            {
                throw new ArgumentNullException("peer is null");
            }

            var ghostType = _QueryGhostType(ghost_base_type);

            var constructor = ghostType.GetConstructor(new[] { typeof(Guid), typeof(bool) });
            if (constructor == null)
            {
                List<string> constructorInfos = new List<string>();

                foreach (var constructorInfo in ghostType.GetConstructors())
                {
                    constructorInfos.Add("(" + constructorInfo.GetParameters() + ")");

                }
                throw new Exception(string.Format("{0} Not found constructor.\n{1}", ghostType.FullName, string.Join("\n", constructorInfos.ToArray())));
            }

            
            var o = constructor.Invoke(new object[] { id, return_type });

            return (IGhost)o;
        }

        public INotifier<T> QueryProvider<T>()
        {
            return _QueryProvider(typeof(T).Name) as INotifier<T>;
        }

        private IProvider _QueryProvider(string type_name)
        {
            IProvider provider = null;
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
            var providerType = providerTemplateType.MakeGenericType(_FindType(type_name));
            return Activator.CreateInstance(providerType) as IProvider;
        }

        private void _ProtocolSubmit(PackageProtocolSubmit data)
        {
            if(_Comparison(_Protocol.VerificationCode, data.VerificationCode) == false)
            {
                OnErrorVerifyEvent?.Invoke(_Protocol.VerificationCode, data.VerificationCode);
            }
        }

        private bool _Comparison(byte[] code1, byte[] code2)
        {
            return new Library.Utility.Comparison<byte>(code1, code2, (arg1, arg2) => arg1 == arg2).Same;
        }

        private void _InvokeEvent(Guid ghost_id, int event_id, byte[][] event_params)
        {
            var ghost = _FindGhost(ghost_id);
            if(ghost != null)
            {
                var map = _Protocol.GetMemberMap();
                var info = map.GetEvent(event_id);

                var type = _GhostProvider.Find(info.DeclaringType);
                var instance = ghost.GetInstance();
                if(type != null)
                {
                    var eventInfos = type.GetField("_" + info.Name, BindingFlags.Instance | BindingFlags.NonPublic);
                    var fieldValue = eventInfos.GetValue(instance);
                    if(fieldValue is Delegate)
                    {
                        var fieldValueDelegate = fieldValue as Delegate;

                        var pars = (from a in event_params
                                    select _Serializer.Deserialize(a)).ToArray();
                        try
                        {
                            fieldValueDelegate.DynamicInvoke(pars);
                        }
                        catch(TargetInvocationException tie)
                        {
                            Regulus.Utility.Log.Instance.WriteInfo(string.Format("Call event error in {0}:{1}. \n{2}", type.FullName, info.Name, tie.InnerException));
                            throw tie;
                        }
                        catch(Exception e)
                        {
                            Regulus.Utility.Log.Instance.WriteInfo(string.Format("Call event error in {0}:{1}.", type.FullName, info.Name));
                            throw e;
                        }
                    }
                }
            }
        }

        private Type _QueryGhostType(Type ghostBaseType)
        {

            if(_GhostProvider.Find(ghostBaseType) == null)
            {

                new AssemblyBuilder().Build(ghostBaseType)
                //codeBuilder.Build(ghostBaseType);

                //codeBuilder.GpiEvent += CodeBuilder_GpiEvent;
                
            }
        }

        private void CodeBuilder_GpiEvent(string type_name, string codes)
        {
            
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
                    _Requester.Request(ClientToServerOpCode.PING, new byte[0]);
                }
            }

            _EndPing();
        }

        protected void _EndPing()
        {
            lock(_Sync)
            {
                if(_PingTimer != null)
                {
                    _PingTimer.Stop();
                    _PingTimer = null;
                }
            }
        }
    }
}
