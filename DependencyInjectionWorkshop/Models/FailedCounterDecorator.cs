namespace DependencyInjectionWorkshop.Models
{
    public class BaseAuthenticationDecorator : IAuthentication
    {
        protected IAuthentication _authentication;

        public BaseAuthenticationDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }

    public class FailedCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            var isLocked = _failedCounter.IsAccountLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
        }

        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);
            return _authentication.Verify(accountId, password, otp);
        }
    }
}