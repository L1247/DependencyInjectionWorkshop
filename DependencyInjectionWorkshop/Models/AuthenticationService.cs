using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao         profileDao;
        private readonly Sha256Adapter      sha256Adapter;
        private readonly OtpProxy           otpProxy;
        private readonly SlackAdapter       slackAdapter;
        private readonly FailedCounterProxy failedCounterProxy;
        private readonly NLogAdapter        nLogAdapter;

        public AuthenticationService()
        {
            profileDao         = new ProfileDao();
            sha256Adapter      = new Sha256Adapter();
            otpProxy           = new OtpProxy();
            slackAdapter       = new SlackAdapter();
            failedCounterProxy = new FailedCounterProxy();
            nLogAdapter        = new NLogAdapter();
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
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isAccountLocked = failedCounterProxy.IsAccountLocked(accountId , httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = sha256Adapter.GetHashedPassword(inputPassword);
            var currentOtp     = otpProxy.GetCurrentOtp(inputOtp , httpClient);
            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                failedCounterProxy.ResetFailCount(accountId , httpClient);
                return true;
            }
            else
            {
                // 驗證失敗，累計失敗次數
                failedCounterProxy.AddFailCount(accountId , httpClient);
                var failedCount = failedCounterProxy.GetFailedCount(accountId , httpClient);
                nLogAdapter.LogFailedCount(accountId , failedCount);
                slackAdapter.NotifyUser(accountId);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}