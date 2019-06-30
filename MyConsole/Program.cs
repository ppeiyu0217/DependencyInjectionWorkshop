using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var authenticationService = new AuthenticationService(new Profile(), new Sha256Adapter(), new OtpService());
            var notificationDecorator = new NotificationDecorator(authenticationService, new SlackAdapter());
            var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator, new FailedCounter());
            var logFailedCount = new LogFailedCountDecorator(failedCounterDecorator, new FailedCounter(), new NLogAdapter());
            var isValid = logFailedCount.Verify("joey", "9487", "9527");
            Console.Write(isValid);
        }
    }
}