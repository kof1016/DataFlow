using Gateway.Framework;

namespace Gateway.Utility
{
    /// <summary>
    /// 更新器
    /// </summary>
    public interface IUpdatable : IBootable
    {
        bool Update();
    }
}