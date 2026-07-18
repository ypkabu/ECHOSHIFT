namespace EchoShift.Reset
{
    public interface IResettable
    {
        void CaptureInitialState();
        void RestoreInitialState();
    }
}
