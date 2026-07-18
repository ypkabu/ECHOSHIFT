using EchoShift.Replay;

namespace EchoShift.Input
{
    public interface IInputSource
    {
        InputCommand Sample(int tick);
    }
}
