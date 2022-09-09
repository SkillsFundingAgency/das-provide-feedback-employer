using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;

namespace ESFA.DAS.Feedback.Employer.Emailer.Configuration
{
    public static class RoutingSettingsExtensions
    {
        private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
        }
    }
}
