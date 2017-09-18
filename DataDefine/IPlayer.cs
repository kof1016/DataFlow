using Library.Synchronize;

using Regulus.Utility;

namespace DataDefine
{
    public interface IPlayer
    {
        Value<bool> IsMain();

        event System.Action<Move> MoveEvent;
    }
}