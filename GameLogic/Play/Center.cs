using System;
using System.Collections.Generic;

using DataDefine;

using Gateway.Framework;
using Gateway.Utility;

using Synchronization.Interface;

namespace GameLogic.Play
{
    public class Center : IUpdatable, IInput
    {
        public event Action<string> OnMainPlayerOpCodeEvent; // TO INPUT

        private readonly ISoulBinder _Binder;

        private readonly List<Player> _Players;

        private readonly Updater _Updater;

        public Center(ISoulBinder binder)
        {
            _Updater = new Updater();
            _Players = new List<Player>();

            _Binder = binder; // server
        }

        void IInput.OpCode(string opcode)
        {
            OnMainPlayerOpCodeEvent?.Invoke(opcode);
        }

        void IBootable.Launch()
        {
            _Binder.Bind<IInput>(this);
        }

        void IBootable.Shutdown()
        {
            _Binder.Unbind<IInput>(this);
        }

        bool IUpdatable.Update()
        {
            _Updater.Working();
            return true;
        }

        public void JoinPlayer(Guid id, int character, bool main_player)
        {
            var player = new Player(_Binder, id, character, main_player);

            _Players.Add(player);

            _Updater.Add(player);
        }

        public void Execute(int player, string op_code)
        {
            var plr = _Players.Find(p => p.Character == player);

            if(op_code == "Forward")
            {
                plr.Forward();
            }
            else if(op_code == "Stop")
            {
                plr.Stop();
            }
        }
    }
}
