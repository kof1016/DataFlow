using System;

using DataDefine;

using Library.Framework;
using Library.Utility;

using Synchronization.Interface;

namespace GameLogic.Play
{
    public class Player : IUpdatable, IPlayer
    {
        private TimeCounter _Counter;
        private readonly ISoulBinder _Binder;

        public readonly Guid Id;

        public readonly int Character;

        private readonly bool _MainPlayer;

        

        private float _PositionX;

        private float _PositionY;

        

        private float _SpeedX;

        private float _SpeedY;

        public Player(ISoulBinder binder, Guid id, int character, bool main_player)
        {
            _Binder = binder;
            Id = id;
            Character = character;
            _MainPlayer = main_player;
            _Counter = new TimeCounter();
        }

        void IBootable.Launch()
        {
            
            _Binder.Bind<IPlayer>(this);
        }

        void IBootable.Shutdown()
        {
            _Binder.Unbind<IPlayer>(this);
            
        }

        bool IUpdatable.Update()
        {
            var delta = _Counter.Second;
            _Counter.Reset();

            _PositionX += _SpeedX * delta;
            _PositionY += _SpeedY * delta;
            return true;
        }

        private event Action<Move> _MoveEvent;

        Library.Synchronize.Value<bool>IPlayer.IsMain(){ 
            return _MainPlayer;
         }

        event Action<Move> IPlayer.MoveEvent
        {
            add
            {
                this._MoveEvent += value;
            }
            remove
            {
                this._MoveEvent -= value;
            }
        }


        public void Forward()
        {
            _SpeedX = 1;
            _SpeedY = 0;


            _MoveEvent.Invoke(new Move(){ DirectionX = _SpeedX  , DirectionY = _SpeedY , StartPositionX = _PositionX , StartPositionY = _PositionY});

            
        }

        public void Stop()
        {
            _SpeedX = 0;
            _SpeedY = 0;

            _MoveEvent.Invoke(new Move() { DirectionX = _SpeedX, DirectionY = _SpeedY, StartPositionX = _PositionX, StartPositionY = _PositionY });
            
        }
    }
}