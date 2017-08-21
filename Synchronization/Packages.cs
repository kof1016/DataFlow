using System;

namespace Synchronization
{
    public class RequestPackage
    {
        public ClientToServerOpCode Code;

        public object Data;
    }

    public class ResponsePackage
    {
        public ServerToClientOpCode Code;

        public object Data;
    }

    public class PackageUpdateProperty 
    {
        public Guid EntityId;

        public int Property;

        public object[] Args;
    }

    public class PackageInvokeEvent
    {
        public Guid EntityId;

        public int Event;

        public byte[][] EventParams;
    }

    public class PackageErrorMethod
    {
        public Guid ReturnTarget;

        public string Method = string.Empty;

        public string Message = string.Empty;
    }

    public class PackageReturnValue
    {
        public Guid ReturnTarget;

        public object[] ReturnValue;
    }

    public class PackageLoadSoulCompile
    {
        public int TypeId;

        public Guid EntityId;

        public Guid ReturnId;
    }

    public class PackageLoadSoul
    {
        public string TypeName;

        public Guid EntityId;

        public bool ReturnType;
    }

    public class PackageUnloadSoul
    {
        public int TypeId;

        public Guid EntityId;
    }

    public class PackageCallMethod
    {
        public Guid EntityId;

        public string MethodName;

        public Guid ReturnId;

        public object[] MethodParams;
    }

    public class PackageProtocolSubmit
    {
        public object[] VerificationCode;
    }

    public class PackageRelease
    {
        public Guid EntityId;
    }

    public class Pkg
    {
        Pkgs[]
    }
}