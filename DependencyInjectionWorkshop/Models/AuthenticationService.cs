using System;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly LogFailedCountDecorator _logFailedCountDecorator;

        public AuthenticationService(ILogger logger, IProfile profile,
            IHash hash,
            IFailedCounter failedCounter,
            IOtpService otpService)
        {
            _logFailedCountDecorator = new LogFailedCountDecorator(this);
            _profile = profile;
            _failedCounter = failedCounter;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _logFailedCountDecorator = new LogFailedCountDecorator(this);
            _profile = new Profile();
            _failedCounter = new FailedCounter();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //取得密碼
            var currentPassword = _profile.GetPassword(accountId);

            var hashPassword = _hash.Compute(password);

            //取得Otp
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            // 驗證密碼、Otp
            if (hashPassword == currentPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                //_logFailedCountDecorator.LogFailedCount(accountId);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}