using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterProxy
    {
        public FailedCounterProxy() { }

        public bool IsAccountLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked" , accountId).GetAwaiter().GetResult();
            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().GetAwaiter().GetResult();
            return isAccountLocked;
        }

        public void Add(string accountId)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add" , accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void Reset(string accountId)
        {
            // 證成功，重設失敗次數
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset" , accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount" , accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}