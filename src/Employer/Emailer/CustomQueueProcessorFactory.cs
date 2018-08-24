using Microsoft.Azure.WebJobs.Host.Queues;

namespace Esfa.Das.Feedback.Employer.Emailer
{
    public class CustomQueueProcessorFactory : IQueueProcessorFactory
    {
        public QueueProcessor Create(QueueProcessorFactoryContext context)
        {
            context.Queue.CreateIfNotExistsAsync().Wait();

            // return the default processor
            return new QueueProcessor(context);
        }
    }
}
