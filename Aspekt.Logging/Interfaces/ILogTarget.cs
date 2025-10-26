namespace Aspekt.Logging.Interfaces
{
    public interface ILogTarget
    {
        void Log(Levels level, string message);
    }
}
