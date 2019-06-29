namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthenticationService
    {
        private readonly INotification _notification;

        public NotificationDecorator(INotification notification)
        {
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