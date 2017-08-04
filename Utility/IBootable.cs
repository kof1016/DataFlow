namespace Utility
{
    /// <summary>
    /// 啟動器
    /// 需要啟動的元件都實作此介面
    /// </summary>
    public interface IBootable
    {
        void Launch();

        void Shutdown();
    }
}