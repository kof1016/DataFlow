using System;

namespace Serialization
{
    public class DescriberException : Exception
    {
        public readonly Type Describer;

        public readonly int Id;

        public readonly Type Instance;

        public readonly Exception Next;

        public DescriberException(Type describer, Type instance, int id, string message)
            : base(message)
        {
            Describer = describer;
            Instance = instance;
            Id = id;
        }

        public DescriberException(Type describer, Type instance, int id, string message, Exception next)
            : base(message)
        {
            Describer = describer;
            Instance = instance;
            Id = id;
            Next = next;
        }
    }
}
