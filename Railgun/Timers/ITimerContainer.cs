using System.Threading.Tasks;
using TreeDiagram.Interfaces;

namespace Railgun.Timers
{
    public interface ITimerContainer
    {
        ITreeTimer Data { get; }
        bool IsCompleted { get; }
        bool HasCrashed { get; }

        void StartTimer(double ms);

        void StopTimer();

        Task ExecuteOverrideAsync();
    }
}