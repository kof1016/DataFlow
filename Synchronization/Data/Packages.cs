using System;

using Gateway.Serialization;
using Gateway.Synchronize;

namespace Synchronization.Data
{
    [Serializable]
    public class RequestPackage
    {
        public ClientToServerOpCode Code;

        public byte[] Data;
    }

    [Serializable]
    public class ResponsePackage
    {
        public ServerToClientOpCode Code;

        public byte[] Data;
    }

    [Serializable]
    public class TPackageData<TData> where TData : class
    {
        public byte[] ToBuffer(ISerializer serializer)
        {
            return serializer.Serialize(this);
        }
    }

    public static class PackageHelper
    {
        public static TData ToPackageData<TData>(this byte[] buffer, ISerializer serializer) where TData : TPackageData<TData>
        {
            return serializer.Deserialize(buffer) as TData;
        }
    }

    [Serializable]
    public class PackageUpdateProperty : TPackageData<PackageUpdateProperty>
    {
        public PackageUpdateProperty()
        {
            Args = new byte[0];
        }

        public int PropertyId;

        public Guid EntityId;

        public byte[] Args;
    }

    public class PackageInvokeEvent : TPackageData<PackageInvokeEvent>
    {
        public PackageInvokeEvent()
        {
            EventParams = new byte[0][];
        }
            
        public int EventId;

        public Guid EntityId;

        public byte[][] EventParams;
    }

    [Serializable]
    public class PackageErrorMethod : TPackageData<PackageErrorMethod>
    {
        public PackageErrorMethod()
        {
            Method = string.Empty;

            Message = string.Empty;
        }

        public Guid ReturnTarget;

        public string Method;

        public string Message;
    }

    [Serializable]
    public class PackageReturnValue : TPackageData<PackageReturnValue>
    {
        public PackageReturnValue()
        {
            ReturnValue = new byte[0];
        }

        public Guid ReturnTarget;

        public byte[] ReturnValue;
    }

    [Serializable]
    public class PackageLoadSoulCompile : TPackageData<PackageLoadSoulCompile>
    {
        public PackageLoadSoulCompile()
        {
        }

        public int TypeId;

        public Guid EntityId;

        public Guid ReturnId;
    }

    [Serializable]
    public class PackageLoadSoul : TPackageData<PackageLoadSoul>
    {
        public PackageLoadSoul()
        {
        }

        public int TypeId;

        public Guid EntityId;

        public bool ReturnType;
    }

    [Serializable]
    public class PackageUnloadSoul : TPackageData<PackageUnloadSoul>
    {
        public PackageUnloadSoul()
        {
        }

        public int TypeId;

        public Guid EntityId;
    }

    [Serializable]
    public class PackageCallMethod : TPackageData<PackageCallMethod>
    {
        public PackageCallMethod()
        {
            MethodParams = new byte[0][];
        }

        public int MethodId;

        public Guid EntityId;

        public Guid ReturnId;

        public byte[][] MethodParams;
    }

    [Serializable]
    public class PackageProtocolSubmit : TPackageData<PackageProtocolSubmit>
    {
        public PackageProtocolSubmit()
        {
        }

        public byte[] VerificationCode;
    }

    [Serializable]
    public class PackageRelease : TPackageData<PackageRelease>
    {
        public PackageRelease()
        {
        }

        public Guid EntityId;
    }
}