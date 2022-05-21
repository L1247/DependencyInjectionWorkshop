using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile       profile;
        private readonly IHash          hash;
        private readonly IOtp           otp;
        private readonly INotification  notification;
        private readonly IFailedCounter failedCounter;
        private readonly ILogger        logger;

        public AuthenticationService(IProfile       profile ,       IHash   hash , IOtp otp , INotification notification ,
                                     IFailedCounter failedCounter , ILogger logger)
        {
            this.profile       = profile;
            this.hash          = hash;
            this.otp           = otp;
            this.notification  = notification;
            this.failedCounter = failedCounter;
            this.logger        = logger;
        }

        public AuthenticationService()
        {
            profile       = new ProfileDao();
            hash          = new Sha256Adapter();
            otp           = new OtpProxy();
            notification  = new SlackAdapter();
            failedCounter = new FailedCounter();
            logger        = new NLogAdapter();
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
            var isAccountLocked = failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = profile.GetPasswordFromDb(accountId);
            var hashedPassword = hash.Compute(inputPassword);
            var currentOtp     = otp.GetCurrentOtp(inputOtp);
            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                // 驗證失敗，累計失敗次數
                failedCounter.Add(accountId);
                var failedCount = failedCounter.Get(accountId);
                logger.LogInfo($"accountId:{accountId} failed times:{failedCount}");
                notification.NotifyUser(accountId);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}