namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter : ILogger
    {
        public NLogAdapter() { }

        public void LogInfo(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}