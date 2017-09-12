using System;
using System.Collections.Generic;
using System.Linq;

using Library.Synchronize;

using Synchronization.Data;
using Synchronization.Interface;

namespace Synchronization
{
    public class AutoRelease
    {
        private readonly Dictionary<Guid, WeakReference> _Exists;

        private readonly IGhostRequest _Requester;

        public AutoRelease(IGhostRequest requester)
        {
            _Requester = requester;
            _Exists = new Dictionary<Guid, WeakReference>();
        }

        public void Register(IGhost ghost)
        {
            var id = ghost.GetID();

            if(_Exists.TryGetValue(id, out WeakReference instance) == false)
            {
                _Exists.Add(id, new WeakReference(ghost));
            }
        }

        public void Update()
        {
            var ids = (from e in _Exists
                       where e.Value.IsAlive == false
                       select e.Key).ToList();

            foreach(var id in ids)
            {
                /*var args = new Dictionary<byte, byte[]>();
				args[0] = id.ToByteArray();*/
                _Exists.Remove(id);

                if(_Requester == null)
                {
                    continue;
                }

                var data = new PackageRelease
                               {
                                   EntityId = id
                               };

                // TODO:
                _Requester.Request(ClientToServerOpCode.Release, null);
            }
        }
    }
}
