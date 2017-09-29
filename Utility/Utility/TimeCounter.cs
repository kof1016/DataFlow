using System;

namespace Gateway.Utility
{
    public class TimeCounter
    {
        private long _Begin;

        public long Ticks => DateTime.Now.Ticks - _Begin;

        public float Second => (float)new TimeSpan(Ticks).TotalSeconds;

        public TimeCounter()
        {
            Reset();
        }

        public void Reset()
        {
            _Begin = DateTime.Now.Ticks;
        }
    }
}
