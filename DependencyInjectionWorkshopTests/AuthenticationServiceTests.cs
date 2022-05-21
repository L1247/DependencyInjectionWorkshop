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
            failedCounter.IsAccountLocked("star").Returns(false);
            profile.GetPasswordFromDb("star").Returns("hashed pw");
            hash.Compute("123").Returns("hashed pw");
            otp.GetCurrentOtp("star").Returns("000000");

            var isValid = authenticationService.Verify("star" , "123" , "000000");
            Assert.AreEqual(true , isValid);

            failedCounter.Received(1).Reset("star");
        }

    #endregion
    }
}