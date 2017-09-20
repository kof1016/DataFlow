using System;

namespace Serialization
{
    public class EnumDescriber : ITypeDescriber
    {
        public EnumDescriber(int id, Type type)
        {
            throw new NotImplementedException();
        }

        int ITypeDescriber.Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Type ITypeDescriber.Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object ITypeDescriber.Default
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        int ITypeDescriber.GetByteCount(object instance)
        {
            throw new NotImplementedException();
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            throw new NotImplementedException();
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instance)
        {
            throw new NotImplementedException();
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {
            throw new NotImplementedException();
        }
    }
}