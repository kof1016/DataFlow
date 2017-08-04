namespace Input.Tests1
{
    public interface ICalculator
    {
        int First { get; set; }

        int Second { get; set; }

        int Total();

        int AddTwoNumber();

        int Add(int i, int i1);
    }

    public class GGININDER : ICalculator
    {
        public int First { get; set; }

        public int Second { get; set; }

        public int Total()
        {
            throw new System.NotImplementedException();
        }

        public int AddTwoNumber()
        {
            throw new System.NotImplementedException();
        }

        public int Add(int i, int i1)
        {
            throw new System.NotImplementedException();
        }
    }
}