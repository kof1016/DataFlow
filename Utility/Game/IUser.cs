using System;

using Library.Utility;

namespace Library.Game
{
    public delegate void NewUser(Guid account);

    public delegate void Quit();

    public delegate void DoneCallback();

    public interface IUser : IUpdatable
    {
        event Quit OnQuitEvent;

        event NewUser OnVerifySuccessEvent;

        void OnKick(Guid id);
    }
}
