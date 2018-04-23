namespace SCIL.Logger
{
    public interface ILogger
    {
        void Log(string message);
        void Log(string message, bool verbose);
        void Wait();
    }
}