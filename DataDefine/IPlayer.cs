using Gateway.Synchronize;

namespace DataDefine
{
    public interface IPlayer
    {
        Value<bool> IsMain();

        event System.Action<Move> MoveEvent;
    }
}