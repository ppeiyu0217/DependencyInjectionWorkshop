namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator
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
        
    }
}