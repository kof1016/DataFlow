using System;
using System.Collections.Generic;
using System.Reflection;

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
            public readonly string PropertyName;

            public readonly PropertyInfo PropertyInfo;

            public object Value;

            public PropertyHandler(PropertyInfo info, string property_name)
            {
                PropertyInfo = info;
                PropertyName = property_name;
            }

            internal bool UpdateProperty(object val)
            {
                // if (!ValueHelper.DeepEqual(Value, val))
                // {
                // Value = ValueHelper.DeepCopy(val);
                // return true;
                // }
                return false;
            }
        }

        public Guid ID { get; set; }

        public object ObjectInstance { get; set; }

        public Type ObjectType { get; set; }

        public MethodInfo[] MethodInfos { get; set; }

        public List<EventHandler> EventHandlers { get; set; }

        public PropertyHandler[] PropertyHandlers { get; set; }

        // public int InterfaceId { get; set; }
        internal void ProcessDiffentValues(Action<Guid, int, object> update_property)
        {
            foreach(var handler in PropertyHandlers)
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
