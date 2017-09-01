using Library.Framework;
using Library.Utility;

namespace Library.Game
{
    public class Hall : IUpdatable
    {
        public event OnNewUser OnNewUserEvent;

        private readonly Updater _Users;

        public Hall()
        {
            _Users = new Updater();
        }

        void IBootable.Launch()
        {
        }

        void IBootable.Shutdown()
        {
        }

        bool IUpdatable.Update()
        {
            _Users.Working();
            return true;
        }

        public void PushUser(IUser user)
        {
            user.OnVerifySuccessEvent += id =>
                {
                    OnNewUserEvent?.Invoke(id);

                    OnNewUserEvent += user.OnKick;
                };

            user.OnQuitEvent += () =>
                {
                    OnNewUserEvent -= user.OnKick;

                    _Users.Remove(user);
                };

            _Users.Add(user);
        }
    }
}
