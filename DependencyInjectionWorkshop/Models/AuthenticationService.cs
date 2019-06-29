using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly INotification _slackAdapter;

        public AuthenticationService(ILogger logger, IProfile profile, INotification slackAdapter,
            IHash sha256Adapter,
            IFailedCounter failedCounter,
            IOtpService otpService)
        {
            _profile = profile;
            _failedCounter = failedCounter;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _logger = logger;
            _slackAdapter = slackAdapter;
        }

        public AuthenticationService()
        {
            _profile = new Profile();
            _failedCounter = new FailedCounter();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _logger = new NLogAdapter();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //檢查帳號是否被封鎖
            var isLocked = _failedCounter.IsAccountLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            //取得密碼
            var currentPassword = _profile.GetPassword(accountId);

            var hashPassword = _sha256Adapter.Compute(password);

            //取得Otp
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            // 驗證密碼、Otp
            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                //累計失敗次數
                _failedCounter.AddFailedCount(accountId);

                //紀錄失敗次數
                var failedCount = _failedCounter.GetFailedCount(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");

                //推播
                _slackAdapter.PushMessage(accountId);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}