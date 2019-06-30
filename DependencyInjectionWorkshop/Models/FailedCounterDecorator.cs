﻿namespace DependencyInjectionWorkshop.Models
{
    public abstract class BaseAuthenticationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected BaseAuthenticationDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }

    public class FailedCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication
            , IFailedCounter failedCounter) : base(authentication)
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

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);
            return base.Verify(accountId, password, otp);
        }
    }
}