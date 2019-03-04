using System.Threading.Tasks;

namespace Railgun.Core.Containers
{
    public interface ITimerContainer
    {
        bool IsCompleted { get; }
        bool HasCrashed { get; }

        void StartTimer(double ms);

        void StopTimer();

        Task ExecuteOverrideAsync();
    }
}
