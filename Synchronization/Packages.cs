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
        public string PropertyName;

        public Guid EntityId;

        public object Arg;
    }

    public class PackageInvokeEvent
    {
        public string EventName;

        public Guid EntityId;

        public object[] EventParams;
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
        public string TypeName;

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
        public string TypeName;

        public Guid EntityId;
    }

    public class PackageCallMethod
    {
        public string MethodName;

        public Guid EntityId;

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
}