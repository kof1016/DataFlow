using System;
using System.Collections.Generic;
using System.Reflection;

using Library.TypeHelper;

namespace Synchronization
{
    public class Soul
    {
        public class EventHandler
        {
            public Delegate DelegateObject;

            public EventInfo EventInfo;
        }

        public class PropertyHandler
        {
            public readonly int Id;

            public readonly PropertyInfo PropertyInfo;

            public object Value;

            public PropertyHandler(PropertyInfo info, int property_id)
            {
                PropertyInfo = info;
                Id = property_id;
            }

            internal bool UpdateProperty(object val)
            {
                
                if (!ValueHelper.DeepEqual(Value, val))
                {
                    Value = ValueHelper.DeepCopy(val);
                    return true;
                }
                return false;
            }
        }

        public Guid ID { get; set; }

        public object ObjectInstance { get; set; }

        public Type ObjectType { get; set; }

        public MethodInfo[] MethodInfos { get; set; }

        public List<EventHandler> EventHandlers { get; set; }

        public PropertyHandler[] PropertyHandlers { get; set; }

        public int InterfaceId { get; set; }

        internal void ProcessDifferentValues(Action<Guid, int, object> update_property)
        {
            foreach (var handler in PropertyHandlers)
            {
                var val = handler.PropertyInfo.GetValue(ObjectInstance, null);

                if(handler.UpdateProperty(val))
                {
                    if(update_property != null)
                    {
                        update_property(ID, handler.Id, val);
                    }
                }
            }
        }
    }
}
