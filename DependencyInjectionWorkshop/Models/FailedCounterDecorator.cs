﻿namespace DependencyInjectionWorkshop.Models
{
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
            var isValid =  base.Verify(accountId, password, otp);
            if (!isValid)
            {
                _failedCounter.AddFailedCount(accountId);
            }
            else
            {
                _failedCounter.ResetFailedCount(accountId);
            }

            return isValid;
        }
    }
}