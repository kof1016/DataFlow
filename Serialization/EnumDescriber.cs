﻿using System;

using Library.Serialization;

namespace Serialization
{
    public class EnumDescriber : ITypeDescriber
    {
        private readonly int _Id;

        private readonly Type _Type;

        private readonly object _Default;

        public EnumDescriber(int id, Type type)
        {
            _Id = id;
            _Type = type;

            _Default = Activator.CreateInstance(type);
        }

        int ITypeDescriber.Id
        {
            get { return _Id; }
        }

        Type ITypeDescriber.Type
        {
            get { return _Type; }
        }

        object ITypeDescriber.Default
        {
            get { return _Default; }
        }

        int ITypeDescriber.GetByteCount(object instance)
        {
            return Varint.GetByteCount(Convert.ToUInt64(instance));
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            return Varint.NumberToBuffer(buffer, begin, Convert.ToUInt64(instance));
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instnace)
        {
            ulong value;
            var readed = Varint.BufferToNumber(buffer, begin, out value);

            instnace = Enum.ToObject(_Type, value);
            return readed;
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {

        }
    }
}