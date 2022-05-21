using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpProxy : IOtp
    {
        public OtpProxy() { }

        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps" , accountId).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode) { }
            else throw new Exception($"web api error, accountId:{accountId}");

            // compare hashed password and otp
            var currentOtp = response.Content.ReadAsAsync<string>().GetAwaiter().GetResult();
            return currentOtp;
        }
    }
}