using System;

using Library.Utility;

namespace Library.Game
{
    public delegate void OnNewUser(Guid account);

    public delegate void OnQuit();

    public delegate void DoneCallback();

    public interface IUser : IUpdatable
    {
        event OnQuit OnQuitEvent;

        event OnNewUser OnVerifySuccessEvent;

        void OnKick(Guid id);
    }
}
