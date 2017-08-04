namespace Utility
{
    public class Updater : Launcher<IUpdatable>
    {
        public void Working()
        {
            foreach (var t in _GetObjectSet())
            {
                if (t.Update() == false)
                {
                    Remove(t);
                }
            }
        }
    }
}