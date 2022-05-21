using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterProxy
    {
        public FailedCounterProxy() { }

        public bool IsAccountLocked(string accountId , HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked" , accountId).GetAwaiter().GetResult();
            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().GetAwaiter().GetResult();
            return isAccountLocked;
        }

        public void AddFailCount(string accountId , HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add" , accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void ResetFailCount(string accountId , HttpClient httpClient)
        {
            // 證成功，重設失敗次數
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset" , accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }
}