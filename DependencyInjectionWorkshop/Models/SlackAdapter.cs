using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(responseMessage => { }, "my channel", $"my message : {accountId}", "my bot name");
        }
    }
}