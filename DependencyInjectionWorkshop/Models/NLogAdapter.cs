namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        public NLogAdapter() { }

        public void LogFailedCount(string accountId , int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}