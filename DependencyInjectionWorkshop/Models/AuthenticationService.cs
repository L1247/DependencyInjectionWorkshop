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
        /// <param name="accountId"></param>
        /// <param name="inputPassword"></param>
        /// <param name="inputOtp"></param>
        /// <returns>IsValid</returns>
        public bool Verify(string accountId , string inputPassword , string inputOtp)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                var password1 = connection.Query<string>("spGetUserPassword" , new { Id = accountId } ,
                    commandType : CommandType.StoredProcedure).SingleOrDefault();

                passwordFromDb = password1;
            }

            var crypt  = new System.Security.Cryptography.SHA256Managed();
            var hash   = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response   = httpClient.PostAsJsonAsync("api/otps" , inputOtp).Result;
            if (response.IsSuccessStatusCode) { }
            else throw new Exception($"web api error, accountId:{inputOtp}");

            // compare hashed password and otp
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}