using DataDefine;

using Library.Framework;
using Library.Game;
using Library.Utility;

using Synchronization;

namespace GameLogic.Play
{
    public class Center : IUpdatable
    {
        private readonly IAccountFinder _AccountFinder;

        private readonly Updater _Updater;

        private readonly Hall _Hall;

        public Center(IAccountFinder account_finder)
        {
            _AccountFinder = account_finder;

            _Hall = new Hall();
            _Updater = new Updater();
        }

        void IBootable.Launch()
        {
            _Updater.Add(_Hall);
        }

        void IBootable.Shutdown()
        {
            _Updater.Shutdown();
        }

        bool IUpdatable.Update()
        {
            _Updater.Working();
            return true;
        }

        public void Join(ISoulBinder binder)
        {
            _Hall.PushUser(new User(binder, _AccountFinder));
        }
    }
}
