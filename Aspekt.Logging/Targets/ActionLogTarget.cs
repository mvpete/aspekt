using Aspekt.Logging.Interfaces;

namespace Aspekt.Logging.Targets
{
    public class ActionLogTarget : ILogTarget
    {
        public Action<Levels, string> OnLog { get; set; } = (l,s)=>{};

        public void Log(Levels level, string message)
        {
            OnLog(level, message);
        }
    }
}
