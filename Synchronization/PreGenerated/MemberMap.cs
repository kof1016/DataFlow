using System;
using System.Collections.Generic;
using System.Reflection;

using Library.TypeHelper;

namespace Synchronization.PreGenerated
{
    public class MemberMap : IEqualityComparer<Type>, IEqualityComparer<PropertyInfo>, IEqualityComparer<EventInfo>, IEqualityComparer<MethodInfo>, IEqualityComparer<int>
    {
        private readonly BilateralMap<int, EventInfo> _Events;

        private readonly BilateralMap<int, Type> _Interfaces;

        private readonly BilateralMap<int, MethodInfo> _Methods;

        private readonly BilateralMap<int, PropertyInfo> _Properties;

        public MemberMap(IEnumerable<MethodInfo> methods, IEnumerable<EventInfo> events, IEnumerable<PropertyInfo> properties, IEnumerable<Type> interfaces)
        {
            _Methods = new BilateralMap<int, MethodInfo>(this, this);
            _Events = new BilateralMap<int, EventInfo>(this, this);
            _Properties = new BilateralMap<int, PropertyInfo>(this, this);
            _Interfaces = new BilateralMap<int, Type>(this, this);

            var id = 0;
            foreach(var method in methods)
            {
                _Methods.Add(++id, method);
            }

            id = 0;
            foreach(var eventInfo in events)
            {
                _Events.Add(++id, eventInfo);
            }

            id = 0;
            foreach(var propertyInfo in properties)
            {
                _Properties.Add(++id, propertyInfo);
            }

            id = 0;
            foreach(var @interface in interfaces)
            {
                _Interfaces.Add(++id, @interface);
            }
        }

        bool IEqualityComparer<EventInfo>.Equals(EventInfo x, EventInfo y)
        {
            return _GetEvent(x.DeclaringType, x.Name) == _GetEvent(y.DeclaringType, y.Name);
        }

        int IEqualityComparer<EventInfo>.GetHashCode(EventInfo obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<int>.Equals(int x, int y)
        {
            return x == y;
        }

        int IEqualityComparer<int>.GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<MethodInfo>.Equals(MethodInfo x, MethodInfo y)
        {
            return _GetMethod(x.DeclaringType, x.Name) == _GetMethod(y.DeclaringType, y.Name);
        }

        int IEqualityComparer<MethodInfo>.GetHashCode(MethodInfo obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<PropertyInfo>.Equals(PropertyInfo x, PropertyInfo y)
        {
            return _GetProperty(x.DeclaringType, x.Name) == _GetProperty(y.DeclaringType, y.Name);
        }

        int IEqualityComparer<PropertyInfo>.GetHashCode(PropertyInfo obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<Type>.Equals(Type x, Type y)
        {
            return x.FullName == y.FullName;
        }

        int IEqualityComparer<Type>.GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }

        public MethodInfo GetMethod(int id)
        {
            _Methods.TryGetItem2(id, out MethodInfo method);
            return method;
        }

        public int GetMethod(MethodInfo method_info)
        {
            _Methods.TryGetItem1(method_info, out int id);
            return id;
        }

        public EventInfo GetEvent(int id)
        {
            _Events.TryGetItem2(id, out EventInfo info);
            return info;
        }

        public int GetEvent(EventInfo info)
        {
            _Events.TryGetItem1(info, out int id);
            return id;
        }

        public int GetProperty(PropertyInfo info)
        {
            _Properties.TryGetItem1(info, out int id);
            return id;
        }

        public PropertyInfo GetProperty(int id)
        {
            _Properties.TryGetItem2(id, out PropertyInfo info);
            return info;
        }

        private string _GetMethod(Type type, string method)
        {
            return $"{type.FullName}.{method}";
        }

        private string _GetEvent(Type type, string name)
        {
            return $"{type.FullName}_{name}";
        }

        private string _GetProperty(Type type, string name)
        {
            return $"{type.FullName}+{name}";
        }

        public Type GetInterface(int type_id)
        {
            _Interfaces.TryGetItem2(type_id, out Type type);
            return type;
        }

        public int GetInterface(Type type)
        {
            _Interfaces.TryGetItem1(type, out int id);
            return id;
        }
    }
}
