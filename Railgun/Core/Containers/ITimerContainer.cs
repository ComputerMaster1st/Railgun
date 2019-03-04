namespace Railgun.Core.Containers
{
    public interface ITimerContainer
    {
        bool IsCompleted { get; }
        bool HasCrashed { get; }
    }
}
