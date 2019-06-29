using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

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