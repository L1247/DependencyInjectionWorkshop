using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        /// <summary>
        /// Password from DB
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                var password = connection.Query<string>("spGetUserPassword" , new { Id = accountId } ,
                    commandType : CommandType.StoredProcedure).SingleOrDefault();

                return password;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string GetHash(string plainText)
        {
            var crypt  = new System.Security.Cryptography.SHA256Managed();
            var hash   = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }

        public string GetOtp(string accountId)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response   = httpClient.PostAsJsonAsync("api/otps" , accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="otp"></param>
        /// <returns>IsValid</returns>
        public bool Verify(string account , string password , string otp)
        {
            return false;
        }
    }
}