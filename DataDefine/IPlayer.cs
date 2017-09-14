using Regulus.Utility;

namespace DataDefine
{
    public interface IPlayer
    {
        event System.Action<Move> MoveEvent;
    }
}