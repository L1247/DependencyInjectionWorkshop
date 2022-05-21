using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpProxy
    {
        public OtpProxy() { }

        public string GetCurrentOtp(string inputOtp , HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps" , inputOtp).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode) { }
            else throw new Exception($"web api error, accountId:{inputOtp}");

            // compare hashed password and otp
            var currentOtp = response.Content.ReadAsAsync<string>().GetAwaiter().GetResult();
            return currentOtp;
        }
    }
}