using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronization.Tests1
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {

            var a = (ulong)Convert.ChangeType(-2100000000 , typeof(long));
            //ulong v = (ulong)(object)3;

            // visual 

            //            var charactor = new Charactor();
            //            var agent = new Agent(new LocalHelper());
            //            var agent = new Agent(new ThreadHelper());
            //            var agent = new Agent(new NetworkHelper());
            //            Guid id = Guid.NewGuid();
            //            agent.CreateGhost<ICharactor>(charactor); // 
            //            string name = "":
            //            agent.Query<ICharactor>().Supply += (c) =>
            //                {
            //                    name = c.Name;
            //                };
            //
            //            Assert.AreEqual("Fightrt" , name  );
            //
            //
            //            var agent = new Peer(new ConnectStage());
            //            var agent = new Peer(new ListenStage());
        }
    }
}
