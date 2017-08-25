using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Input.Tests1
{

    public interface IVerify
    {

        event Action<int> AEvent;
    }

    public class Verify : IVerify
    {
        public event Action<int> OnAEvent;

        event Action<int> IVerify.AEvent
        {
            add => OnAEvent += value;
            remove => OnAEvent -= value;
        }

        public void Login()
        {
            OnAEvent(100);
        }
    }

    //如果實作一個事件，可能是怎麼樣
    /*
     * public class event
     * {
     *      public void Invoke(int a)
     *      {
     *          ...
     *      }
     *      
     * }
     */

        
     /*
      * lambda是什麼
      * 可以用一個class實作
      * 
     */

    public class Start
    {
        private Verify _Verify = new Verify();
        public Start()
        {
            _Verify.OnAEvent += (i) =>
                {
                    _SendToClient(
                                  Guid.NewGuid(), new object[]
                                                      {
                                                          i
                                                      });

                };

        }

        public void _SendToClient(Guid instance_id, object[] args)
        {
            //send pkg;

        }
    }

    public class Binder<T>
    {
        void Bind<T>(T verify)
        {
            foreach(var e in typeof(T).GetEvents())
            {
                var method = new GenericEventClosure<>(Guid.NewGuid(), string event_name, _SendToClient);
                e.AddMethod();  // add
                e.RemoveMethod(); // remove

            }
            
        }

       
    }

    

    public delegate void InvokeEventCallback(Guid entity_id, string event_name, object[] args);

    public class GenericEventClosure
    {
        private readonly Guid _EntityId;

        private readonly int _EventId;

        private readonly InvokeEventCallback _InvokeEvent;

        public GenericEventClosure(Guid entity_id, int event_id, InvokeEventCallback invoke_event)
        {
            _EntityId = entity_id;
            _EventId = event_id;
            _InvokeEvent = invoke_event;
        }

        public static Type GetDelegateType()
        {
            return typeof(Action);
        }

        public void Run()
        {
            _InvokeEvent(
                         _EntityId,
                         _EventId,
                         new object[]
                             {
                             });
        }
    }



    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            //Assert.Pass("Your first passing test");

            Assert.AreNotEqual(0, 1);

            Print(out int a);
            Console.WriteLine(a);
            Console.WriteLine($"{a}");
        }

        public void Print(out int a)
        {
            a = 0;
        }
    }
}
