namespace Synchronization
{
    public enum ClientToServerOpCode
    {
        CALL_METHOD = 1,

        PING,

        RELEASE
    }

    public enum ServerToClientOpCode
    {
        InvokeEvent = 1,

        LoadSoul,

        UnloadSoul,

        ReturnValue,

        UpdateProperty,

        LoadSoulCompile,

        PING,

        ErrorMethod,

        ProtocolSubmit
    }
}



    