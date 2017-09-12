using NSubstitute;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace GameLogic.Tests1
{
    [Binding]
    public class SpecFlowFeature1Steps
    {
        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int p0)
        {
            ICalculator cal = Substitute.For<ICalculator>();
            cal.First = p0;

        }

        [Given(@"I have also entered (.*) into the calculator")]
        public void GivenIHaveAlsoEnteredIntoTheCalculator(int p0)
        {
            ICalculator cal = Substitute.For<ICalculator>();
            cal.Second = p0;

            var calculator = Substitute.For<ICalculator>();

            //calculator.Received().Add(1, Arg.Any<int>());

            //calculator.Add(1, 2);
            //calculator.Received().Add(1, 2);

            //calculator.Add(2, 2);
            //calculator.Received().Add(3, 1);
            //calculator.DidNotReceive().Add(5, 7);

            //calculator.Received().Add(1, 1);
  //          calculator.Received().Add(10, 10);
//            calculator.Received().Add(10, Arg.Any<int>());
            calculator.Add(10, -5);
            
            calculator.Received().Add(10, Arg.Any<int>()); //true
            calculator.Received().Add(10, Arg.Is<int>(x => x < 0));//true

            calculator.Received().Add(10, 10); // false
        }

        [Given(@"我輸入 (.*)")]
        public void Given我輸入(int p0)
        {
            Assert.AreEqual(p0, 100);
        }



        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            ICalculator cal = Substitute.For<ICalculator>();
            cal.AddTwoNumber();
        }
        
        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            ICalculator cal = Substitute.For<ICalculator>();
            Assert.AreEqual(cal.Total(), p0);
            
        }
    }
}
