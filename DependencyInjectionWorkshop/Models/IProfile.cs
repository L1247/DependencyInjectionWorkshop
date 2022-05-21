namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPasswordFromDb(string accountId);
    }
}