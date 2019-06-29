using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfile _profile;
        private IFailedCounter _failedCounter;
        private IHash _sha256Adapter;
        private ILogger _logger;
        private INotification _slackAdapter;
        private IOtpService _otpService;
        private AuthenticationService _authenticationService;
        private string DefaultAccount = "joey";
        private string DefaultInputPassword = "9487";
        private string DefaultOtp = "9527";
        private string DefaultHashPassword = "abc";

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _sha256Adapter = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _slackAdapter = Substitute.For<INotification>();
            _otpService = Substitute.For<IOtpService>();

            _authenticationService = new AuthenticationService(
                _logger,
                _profile,
                _slackAdapter,
                _sha256Adapter,
                _failedCounter,
                _otpService);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashPassword(DefaultInputPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultInputPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }


        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            return isValid;
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).ReturnsForAnyArgs(otp);
        }

        private void GivenHashPassword(string password, string hashedPassword)
        {
            _sha256Adapter.Compute(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPasswordFromDb(string accountId, string passwordFromDb)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(passwordFromDb);
        }
    }
}