using System;
using System.Collections.Generic;

namespace Synchronization
{
    public class GhostInterfaceProvider
    {
        private readonly Dictionary<Type, Type> _Types = new Dictionary<Type, Type>();

        public GhostInterfaceProvider()
        {
        }

        public void Add(Type ghost_base_type, Type ghost_type)
        {
            //ghost_type.GetInterface(ghost_base_type.Name);
            // can be use list

            _Types.Add(ghost_base_type, ghost_type);
        }

        public Type Find(Type ghost_base_type)
        {
            if (_Types.ContainsKey(ghost_base_type))
            {
                return _Types[ghost_base_type];
            }

            return null;
        }
    }
}