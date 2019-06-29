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

        private void NotificationWhenInvalid(string accountId)
        {
            _notification.PushMessage(accountId);
        }

        public bool Verify(string accountId, string password, string otp)
        {
            throw new System.NotImplementedException();
        }
    }
}