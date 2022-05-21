namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        bool IsAccountLocked(string accountId);
        void Add(string             accountId);
        void Reset(string           accountId);
        int  Get(string             accountId);
    }
}