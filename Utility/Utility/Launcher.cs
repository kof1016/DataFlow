using System;
using System.Collections.Generic;

using Library.Framework;

namespace Library.Utility
{
    public class Launcher<T> where T : IBootable
    {
        public event Action<T> OnAddEvent;

        public event Action<T> OnRemoveEvent;

        private readonly Queue<T> _Launchers = new Queue<T>();

        private readonly Queue<T> _Adds = new Queue<T>();

        private readonly Queue<T> _Removes = new Queue<T>();

        private readonly List<T> _Ts = new List<T>();

        public int Count => _Ts.Count;

        private IEnumerable<T> _Objects => _Ts.ToArray();

        protected IEnumerable<T> _GetObjectSet()
        {
            lock(_Ts)
            {
                lock(_Removes)
                {
                    _Remove(_Removes, _Ts);
                }

                lock(_Adds)
                {
                    _Add(_Adds, _Ts);
                }

                return _Objects;
            }
        }

        public void Add(T boot)
        {
            lock(_Adds)
            {
                _Adds.Enqueue(boot);
            }
        }

        public void Remove(T boot)
        {
            lock(_Removes)
            {
                _Removes.Enqueue(boot);
            }
        }

        private void _Add(Queue<T> launchers, List<T> boots)
        {
            while(launchers.Count > 0)
            {
                var launcher = launchers.Dequeue();
                boots.Add(launcher);
                launcher.Launch();

                OnAddEvent?.Invoke(launcher);
            }
        }

        private void _Remove(Queue<T> launchers, List<T> boots)
        {
            while(launchers.Count > 0)
            {
                var launcher = launchers.Dequeue();
                boots.Remove(launcher);
                launcher.Shutdown();

                OnRemoveEvent?.Invoke(launcher);
            }
        }

        public void Shutdown()
        {
            lock(_Ts)
            {
                _Shutdown(_Ts);
                _Ts.Clear();
            }
        }

        private void _Shutdown(List<T> boots)
        {
            foreach(var o in boots)
            {
                o.Shutdown();
            }
        }
    }
}
