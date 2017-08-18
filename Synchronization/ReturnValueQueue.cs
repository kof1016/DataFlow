using System;
using System.Collections.Generic;

using Library.Synchronize;

namespace Synchronization
{
    public class ReturnValueQueue
    {
        private readonly Dictionary<Guid, IValue> _ReturnValues = new Dictionary<Guid, IValue>();

        public Guid PushReturnValue(IValue value)
        {
            var id = Guid.NewGuid();
            _ReturnValues.Add(id, value);
            return id;
        }

        internal IValue PopReturnValue(Guid return_target)
        {
            return _PopReturnValue(return_target);
        }

        private IValue _PopReturnValue(Guid return_target)
        {
            if(_ReturnValues.TryGetValue(return_target, out IValue val))
            {
                _ReturnValues.Remove(return_target);
            }

            return val;
        }
    }
}
