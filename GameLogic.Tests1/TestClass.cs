using System;

using NUnit.Framework;

namespace GameLogic.Tests1
{

   



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
