using System.Collections.Generic;

namespace Library.Utility
{
    public class StateMachine
    {
        private Queue<IStage> _StandBys;

        public IStage Current { get; private set; }

        public StateMachine()
        {
            _StandBys = new Queue<IStage>();
        }

        public void Push(IStage new_stage)
        {
            _StandBys.Enqueue(new_stage);
        }

        public bool Update()
        {
            _SetCurrentState();
            _UpdateCurrentStage();

            return Current != null;
        }

        public void Termination()
        {
            _StandBys.Clear();

            if(Current == null)
            {
                return;
            }

            Current.Leave();
            Current = null;
        }

        public void Empty()
        {
            Push(new EmptyStage());
        }

        private void _SetCurrentState()
        {
            var state = _StandBys.Dequeue();
            
            if (state == null)
            {
                return;
            }

            Current?.Leave();

            state.Enter();
            Current = state;
        }

        private void _UpdateCurrentStage()
        {
            Current?.Update();
        }
    }
}