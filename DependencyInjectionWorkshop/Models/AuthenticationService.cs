using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly FailedCounter _failedCounter;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly NLogAdapter _nLogAdapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService(ProfileDao profileDao, FailedCounter failedCounter, Sha256Adapter sha256Adapter, OtpService otpService, NLogAdapter nLogAdapter, SlackAdapter slackAdapter)
        {
            _profileDao = profileDao;
            _failedCounter = failedCounter;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _nLogAdapter = nLogAdapter;
            _slackAdapter = slackAdapter;
        }
        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _failedCounter = new FailedCounter();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _nLogAdapter = new NLogAdapter();
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
            var currentPassword = _profileDao.GetPassword(accountId);

            var hashPassword = _sha256Adapter.Hash(password);

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
                _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");

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