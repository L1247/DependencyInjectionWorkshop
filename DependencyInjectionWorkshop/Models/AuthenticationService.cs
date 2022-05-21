using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao    profileDao;
        private readonly Sha256Adapter sha256Adapter;
        private readonly OtpProxy      otpProxy;

        public AuthenticationService()
        {
            profileDao    = new ProfileDao();
            sha256Adapter = new Sha256Adapter();
            otpProxy      = new OtpProxy();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="inputPassword"></param>
        /// <param name="inputOtp"></param>
        /// <returns>IsValid</returns>
        public bool Verify(string accountId , string inputPassword , string inputOtp)
        {
            var httpClient       = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isAccountLocked = IsAccountLocked(accountId , httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = sha256Adapter.GetHashedPassword(inputPassword);
            var currentOtp     = otpProxy.GetCurrentOtp(inputOtp , httpClient);
            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                ResetFailCount(accountId , httpClient);
                return true;
            }
            else
            {
                // 驗證失敗，累計失敗次數
                AddFailCount(accountId , httpClient);
                var failedCount = GetFailedCount(accountId , httpClient);
                LogFailedCount(accountId , failedCount);
                NotifyUser(accountId);
                return false;
            }
        }

        private static bool IsAccountLocked(string accountId , HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked" , accountId).GetAwaiter().GetResult();
            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().GetAwaiter().GetResult();
            return isAccountLocked;
        }

        private static void NotifyUser(string accountId)
        {
            var message     = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { } , "my channel" , message , "my bot name");
        }

        private static void LogFailedCount(string accountId , int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId , HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount" , accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailCount(string accountId , HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add" , accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailCount(string accountId , HttpClient httpClient)
        {
            // 證成功，重設失敗次數
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset" , accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}