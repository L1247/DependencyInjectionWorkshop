using System;
using DependencyInjectionWorkshop.Models;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var authenticationService = new AuthenticationService();
            var account               = "rStar";
            var password              = "123";
            var otp                   = "000000";
            var isValid               = authenticationService.Verify(account , password , otp);
            Assert.AreEqual(true , isValid);
        }
    }
}