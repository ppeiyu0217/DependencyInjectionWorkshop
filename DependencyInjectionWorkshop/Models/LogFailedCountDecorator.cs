using System.Runtime.Serialization;

namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter,
            ILogger logger) : base(authentication)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogFailedCount(accountId);
            }

            return isValid;
        }
    }
}