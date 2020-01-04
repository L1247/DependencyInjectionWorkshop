﻿using System;
using System.Net.Http;

#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpService _otpService;
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};

            //check account locked
            var isLocked = _failedCounter.GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId, httpClient);

                return true;
            }
            else
            {
                //失敗
                _failedCounter.AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        /// <summary>
        ///     Logs the failed count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        private void LogFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數 
            var failedCount = _failedCounter.GetFailedCount(accountId, httpClient);
            _nLogAdapter.Info(accountId, failedCount);
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}