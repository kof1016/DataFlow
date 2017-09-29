using System;

using Gateway.Synchronize;
using Gateway.Utility;

namespace Synchronization.Interface
{
    public interface IGhostQuerier
    {
        /// <summary>
        ///     查詢介面物件通知者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        INotifier<T> QueryNotifier<T>();
    }
    public interface IAgent : IUpdatable, IGhostQuerier
    {
        /// <summary>
        ///     與遠端發生斷線
        ///     呼叫Disconnect不會發生此事件
        /// </summary>
        event Action BreakEvent;

        /// <summary>
        ///     連線成功事件
        /// </summary>
        event Action ConnectEvent;

        /// <summary>
        ///     錯誤的方法呼叫
        ///     如果呼叫的方法參數有誤則會回傳此訊息.
        ///     事件參數:
        ///     1.方法名稱
        ///     2.錯誤訊息
        ///     會發生此訊息通常是因為client與server版本不相容所致.
        /// </summary>
        event Action<string, string> ErrorMethodEvent;

        /// <summary>
        ///     驗證錯誤
        ///     代表與伺服器端的驗證碼不符
        ///     事件參數:
        ///     1.伺服器驗證碼
        ///     2.本地驗證碼
        ///     會發生此訊息通常是因為client與server版本不相容所致.
        /// </summary>
        event Action<string, string> ErrorVerifyEvent;

        /// <summary>
        ///     Ping
        /// </summary>
        long Ping { get; }

        /// <summary>
        ///     是否為連線狀態
        /// </summary>
        bool Connected { get; }

        

        /// <summary>
        ///     連線
        /// </summary>
        /// <param name="ip_address"></param>
        /// <param name="port"></param>
        /// <returns>如果連線成功會發生OnValue傳回true</returns>
        Value<bool> Connect(string ip_address, int port);

        /// <summary>
        ///     斷線
        /// </summary>
        void Disconnect();
    }
}
