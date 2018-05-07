using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;

namespace InjectStorageContext.Pipeline
{
    public class StorageContextBehavior : Behavior<IInvokeHandlerContext>
    {
        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            var builder = context.Builder;
            var storageContext = builder.Build<StorageContext>();

            try
            {
                var session = context.SynchronizedStorageSession.SqlPersistenceSession();
                storageContext.Connection = session.Connection;
                storageContext.Transaction = session.Transaction;

                await next().ConfigureAwait(false);
            }
            finally
            {
                builder.Release(storageContext);
            }
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base(typeof(StorageContextBehavior).Name, typeof(StorageContextBehavior), "Database context") {}
        }
    }
}