namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthenticationService authenticationService,INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid)
            {
                _notification.PushMessage(accountId);
            }

            return isValid;
        }
    }
}