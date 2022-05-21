namespace DependencyInjectionWorkshop.Models
{
    public interface IOtp
    {
        string GetCurrentOtp(string inputOtp);
    }
}