using System;

namespace Utility
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
