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
            var account               = Guid.NewGuid().ToString();
            var password              = Guid.NewGuid().ToString();
            var otp                   = Guid.NewGuid().ToString();
            var isValid               = authenticationService.Verify(account , password , otp);
            Assert.AreEqual(true , isValid);
        }
    }
}