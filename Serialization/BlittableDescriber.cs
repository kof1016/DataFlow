using System;
using System.Runtime.InteropServices;

namespace Serialization
{
    public class BlittableDescriber<T> : BlittableDescriber
    {
        public BlittableDescriber(int id)
            : base(id, typeof(T))
        {
        }
    }

    public class BlittableDescriber : ITypeDescriber
    {
        private readonly object _Default;

        private readonly int _Id;

        private readonly Type _Type;

        private readonly int _Size;

        public BlittableDescriber(int id, Type type)
        {
            _Id = id;
            _Type = type;

            _Default = Activator.CreateInstance(type);
            try
            {
                _Size = Marshal.SizeOf(_Type);
            }
            catch(Exception ex)
            {
                throw new DescriberException(typeof(BlittableDescriber), _Type, _Id, "Size", ex);
            }
        }

        int ITypeDescriber.Id => _Id;

        Type ITypeDescriber.Type => _Type;

        object ITypeDescriber.Default => _Default;

        int ITypeDescriber.GetByteCount(object instance)
        {
            return _Size;
        }

        int ITypeDescriber.ToBuffer(object instance, byte[] buffer, int begin)
        {
            return _ToBuffer(instance, buffer, begin);
        }

        int ITypeDescriber.ToObject(byte[] buffer, int begin, out object instnace)
        {
            try
            {
                var size = Marshal.SizeOf(_Type);

                var ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(buffer, begin, ptr, size);

                instnace = Marshal.PtrToStructure(ptr, _Type);
                Marshal.FreeHGlobal(ptr);
                return size;
            }
            catch(Exception ex)
            {
                throw new DescriberException(typeof(BlittableDescriber), _Type, _Id, "ToObject", ex);
            }
        }

        void ITypeDescriber.SetMap(TypeSet type_set)
        {
        }

        private int _ToBuffer(object instance, byte[] buffer, int begin)
        {
            int readCount;
            var pinStructure = GCHandle.Alloc(instance, GCHandleType.Pinned);
            try
            {
                readCount = _Size;
                Marshal.Copy(pinStructure.AddrOfPinnedObject(), buffer, begin, readCount);
            }
            catch(Exception ex)
            {
                throw new DescriberException(typeof(BlittableDescriber), _Type, _Id, "ToBuffer", ex);
            }
            finally
            {
                if(pinStructure.IsAllocated)
                {
                    pinStructure.Free();
                }
            }

            return readCount;
        }
    }
}
