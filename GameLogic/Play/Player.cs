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

        public readonly bool MainPlayer;

        private float _PositionX;

        private float _PositionY;

        

        private float _SpeedX;

        private float _SpeedY;

        public Player(ISoulBinder binder, Guid id, int character)
        {
            _Binder = binder;
            Id = id;
            Character = character;
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
            _MoveEvent.Invoke(new Move(){ DirectionX = _SpeedX  , DirectionY = _SpeedY , StartPositionX = _PositionX , StartPositionY = _PositionY});

            _SpeedX = 1;
            _SpeedY = 0;
        }

        public void Stop()
        {
            _MoveEvent.Invoke(new Move() { DirectionX = _SpeedX, DirectionY = _SpeedY, StartPositionX = _PositionX, StartPositionY = _PositionY });

            _SpeedX = 0;
            _SpeedY = 0;
        }
    }
}