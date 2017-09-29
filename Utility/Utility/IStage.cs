namespace Gateway.Utility
{
    public interface IStage
    {
        void Enter();

        void Leave();

        void Update();
    }
}