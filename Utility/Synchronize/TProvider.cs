using System;
using System.Collections.Generic;
using System.Linq;

namespace Gateway.Synchronize
{
    public class TProvider<T> : INotifier<T>, IProvider where T : class
    {
        private event Action<T> _OnReturnEvent;

        private event Action<T> _OnSupplyEvent;

        private event Action<T> _OnUnsupplyEvent;

        private readonly List<T> _Entities = new List<T>();

        private readonly List<WeakReference> _Returns = new List<WeakReference>();

        private readonly List<T> _Waits = new List<T>();

        event Action<T> INotifier<T>.Return
        {
            add => _OnReturnEvent += value;
            remove => _OnReturnEvent -= value;
        }

        event Action<T> INotifier<T>.Supply
        {
            add
            {
                _OnSupplyEvent += value;

                lock(_Entities)
                {
                    foreach(var e in _Entities.ToArray())
                    {
                        value(e);
                    }
                }
            }

            remove => _OnSupplyEvent -= value;
        }

        event Action<T> INotifier<T>.Unsupply
        {
            add => _OnUnsupplyEvent += value;
            remove => _OnUnsupplyEvent -= value;
        }

        T[] INotifier<T>.Ghosts
        {
            get
            {
                lock(_Entities)
                {
                    return _Entities.ToArray();
                }
            }
        }

        T[] INotifier<T>.Returns => _RemoveNoReferenceReturns();

        /// <summary>
        ///     IProvider
        /// </summary>
        void IProvider.Add(IGhost entity)
        {
            _Waits.Add(entity as T);
        }

        void IProvider.Remove(Guid id)
        {
            _RemoveNoReferenceReturns();

            _RemoveEntities();

            _RemoveWaits();

            _RemoveReturns();

            void _RemoveEntities()
            {
                lock(_Entities)
                {
                    var entity = (from e in _Entities
                                  where (e as IGhost).GetID() == id
                                  select e).FirstOrDefault();

                    if(entity != null && _OnUnsupplyEvent != null)
                    {
                        _OnUnsupplyEvent.Invoke(entity);
                    }

                    _Entities.Remove(entity);
                }
            }

            void _RemoveWaits()
            {
                var waitEntity = (from e in _Waits
                                  where (e as IGhost).GetID() == id
                                  select e).FirstOrDefault();
                if(waitEntity != null)
                {
                    _Waits.Remove(waitEntity);
                }
            }

            void _RemoveReturns()
            {
                var entity = (from weakRef in _Returns
                              let e = weakRef.Target as IGhost
                              where weakRef.IsAlive && e.GetID() == id
                              select weakRef).SingleOrDefault();

                if(entity != null)
                {
                    _Returns.Remove(entity);
                }
            }
        }

        IGhost IProvider.Ready(Guid id)
        {
            var entity = (from e in _Waits
                          where (e as IGhost).GetID() == id
                          select e).FirstOrDefault();

            _Waits.Remove(entity);

            if(entity != null)
            {
                return _Add(entity, entity as IGhost);
            }

            return null;

            IGhost _Add(T e, IGhost g)
            {
                if(g.IsReturnType() == false)
                {
                    lock(_Entities)
                    {
                        _Entities.Add(e);
                    }

                    _OnSupplyEvent?.Invoke(e);
                }
                else
                {
                    _Returns.Add(new WeakReference(e));
                    _OnReturnEvent?.Invoke(e);
                }

                return g;
            }
        }

        void IProvider.ClearGhosts()
        {
            _RemoveNoReferenceReturns();

            if(_OnUnsupplyEvent != null)
            {
                foreach(var e in _Entities)
                {
                    _OnUnsupplyEvent.Invoke(e);
                }
            }

            _Entities.Clear();
            _Waits.Clear();
            _Returns.Clear();
        }

        IGhost[] IProvider.Ghosts
        {
            get
            {
                var all = _Entities.Concat(_Waits)
                                   .Concat(
                                           from r in _Returns
                                           where r.IsAlive
                                           select r.Target as T);

                return (from entity in all
                        select (IGhost)entity).ToArray();
            }
        }

        private T[] _RemoveNoReferenceReturns()
        {
            var alives = (from w in _Returns
                          where w.IsAlive
                          select w.Target as T).ToArray();
            _Returns.RemoveAll(w => w.IsAlive == false);
            return alives;
        }
    }
}
