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
        ///
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="otp"></param>
        /// <returns>IsValid</returns>
        public bool Verify(string account , string password , string otp)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                var password1 = connection.Query<string>("spGetUserPassword" , new { Id = account } ,
                    commandType : CommandType.StoredProcedure).SingleOrDefault();

                passwordFromDb = password1;
            }

            var crypt  = new System.Security.Cryptography.SHA256Managed();
            var hash   = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response   = httpClient.PostAsJsonAsync("api/otps" , otp).Result;
            if (response.IsSuccessStatusCode) { }
            else throw new Exception($"web api error, accountId:{otp}");

            var currentOtp = response.Content.ReadAsAsync<string>().Result;

        }
    }
}