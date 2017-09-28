using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Protocol;

using Serialization;

using TrueSync;

namespace Synchronization.Tests1
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {

            
            var disintegrator = new TypeDisintegrator(typeof(TSVector));
            Assert.True(true);
        }
    }
}
