using System;
using System.Collections.Generic;

using DataDefine;

using Library.Framework;
using Library.Utility;

using Synchronization.Interface;

using SyncLocal;

namespace GameLogic.Play
{
    public class Center : IUpdatable , IInput
    {
        public event Action<string> OnMainPlayerOpCodeEvent; // TO INPUT

        private readonly List<Player> _Players;

        private readonly Updater _Updater;

        private readonly ISoulBinder _Binder;

        

        public Center(ISoulBinder binder)
        {
            _Updater = new Updater();
            _Players = new List<Player>();
            //var agent = new Agent();

            
            _Binder = binder; // server
            

            //_Updater.Add(new Logic(_Center.Binder, _Viewer, _Connector));
            //_Updater.Add(new Visual(_Center.GhostQuerier, _Command, _Viewer));
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
            
            var player = new Player(_Binder, id, character , main_player);

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

        void IInput.OpCode(string opcode)
        {
            OnMainPlayerOpCodeEvent(opcode);
        }
    }
}
