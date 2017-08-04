namespace Utility
{
    using System.CodeDom;

    /// <summary>
    /// 更新器
    /// </summary>
    public interface IUpdatable : IBootable
    {
        bool Update();
    }
}