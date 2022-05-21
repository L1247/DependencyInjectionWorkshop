﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                var password1 = connection.Query<string>("spGetUserPassword" , new { Id = accountId } ,
                    commandType : CommandType.StoredProcedure).SingleOrDefault();

                passwordFromDb = password1;
            }

            return passwordFromDb;
        }
    }

    public class AuthenticationService
    {
        private ProfileDao profileDao;

        public AuthenticationService()
        {
            profileDao = new ProfileDao();
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
            var hashedPassword = GetHashedPassword(inputPassword);
            var currentOtp     = GetCurrentOtp(inputOtp , httpClient);
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

        private static string GetHashedPassword(string inputPassword)
        {
            var crypt  = new System.Security.Cryptography.SHA256Managed();
            var hash   = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }

        private static string GetCurrentOtp(string inputOtp , HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps" , inputOtp).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode) { }
            else throw new Exception($"web api error, accountId:{inputOtp}");

            // compare hashed password and otp
            var currentOtp = response.Content.ReadAsAsync<string>().GetAwaiter().GetResult();
            return currentOtp;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}