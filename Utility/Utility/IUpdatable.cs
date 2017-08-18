using Library.Framework;

namespace Library.Utility
{
    /// <summary>
    /// 更新器
    /// </summary>
    public interface IUpdatable : IBootable
    {
        bool Update();
    }
}