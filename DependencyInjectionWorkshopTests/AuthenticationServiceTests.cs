#region

using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

#endregion

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
    #region Private Variables

        private AuthenticationService authenticationService;
        private IFailedCounter        failedCounter;
        private IHash                 hash;
        private ILogger               logger;
        private INotification         notification;
        private IOtp                  otp;
        private IProfile              profile;

    #endregion

    #region Setup/Teardown Methods

        [SetUp]
        public void SetUp()
        {
            profile               = Substitute.For<IProfile>();
            hash                  = Substitute.For<IHash>();
            otp                   = Substitute.For<IOtp>();
            notification          = Substitute.For<INotification>();
            failedCounter         = Substitute.For<IFailedCounter>();
            logger                = Substitute.For<ILogger>();
            authenticationService = new AuthenticationService(profile , hash , otp , notification , failedCounter , logger);
        }

    #endregion

    #region Test Methods

        [Test]
        public void Valid()
        {
            WhenValid();
            ShouldValid();

            // failedCounter.Received(1).Reset("star");
        }

        private void WhenValid()
        {
            GivenIsAccountLocked("star" , false);
            GivenPasswordFromDb("star" , "hashed pw");
            GivenHashedPassword("123" , "hashed pw");
            GivenCurrentOtp("star" , "000000");
        }

        private void ShouldValid()
        {
            var isValid = authenticationService.Verify("star" , "123" , "000000");
            Assert.AreEqual(true , isValid);
        }

        private void GivenCurrentOtp(string accountId , string currentOtp)
        {
            otp.GetCurrentOtp(accountId).Returns(currentOtp);
        }

        private void GivenHashedPassword(string inputPassword , string hashedResult)
        {
            hash.Compute(inputPassword).Returns(hashedResult);
        }

        private void GivenPasswordFromDb(string accountId , string password)
        {
            profile.GetPasswordFromDb(accountId).Returns(password);
        }

        private void GivenIsAccountLocked(string accountId , bool isLocked)
        {
            failedCounter.IsAccountLocked(accountId).Returns(isLocked);
        }

    #endregion
    }
}