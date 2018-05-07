using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using InjectStorageContext.Messages;
using InjectStorageContext.Pipeline;
using InjectStorageContext.Repositories;
using NServiceBus;
using NServiceBus.Persistence.Sql;

namespace InjectStorageContext
{
    internal class Program
    {
        public const string TransportConnectionString = @"Server=localhost\sqlexpress2012;Database=nservicebus;Trusted_Connection=True;App=Transport"; // Using different App values prevents lightweight transactions
        public const string PersistenceConnectionString = @"Server=localhost\sqlexpress2012;Database=nservicebus;Trusted_Connection=True;App=Persistence";
        private const string Title = "InjectStorageContext";

        private static async Task Main()
        {
            Console.Title = Title;

            var endpointConfiguration = new EndpointConfiguration(Title);
            endpointConfiguration.UseSerialization<JsonSerializer>();

            // Transport

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(TransportConnectionString);

            // Persistence
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(PersistenceConnectionString));
            persistence.SubscriptionSettings().CacheFor(TimeSpan.FromMinutes(1));

            endpointConfiguration.EnableOutbox();

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            // Dependency injection registrations
            var builder = new ContainerBuilder();
            builder.RegisterType<OrderRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<StorageContext>().AsSelf().InstancePerLifetimeScope();
            endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(builder.Build()));

            // Pipeline

            endpointConfiguration.Pipeline.Register<StorageContextBehavior.Registration>();

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            try
            {
                Console.WriteLine("Press:");
                Console.WriteLine("[1] to send OrderSubmitted");
                Console.WriteLine("[2] to send OrderSubmittedWithError");
                Console.WriteLine("[3] to send BulkOrderSubmittedWithError");
                Console.WriteLine("[ESC] to exit");

                ConsoleKey key;
                while ((key = Console.ReadKey(true).Key) != ConsoleKey.Escape)
                {
                    switch (key)
                    {
                        case ConsoleKey k when k == ConsoleKey.D1 || k == ConsoleKey.NumPad1:
                            await SendOrderSubmitted(endpointInstance);
                            break;
                        case ConsoleKey k when k == ConsoleKey.D2 || k == ConsoleKey.NumPad2:
                            await SendOrderSubmittedWithError(endpointInstance);
                            break;
                        case ConsoleKey k when k == ConsoleKey.D3 || k == ConsoleKey.NumPad3:
                            await SendBulkOrderSubmittedWithError(endpointInstance);
                            break;
                        default:
                            Console.WriteLine("Invalid key.");
                            break;
                    }

                   
                }
            }
            finally
            {
                await endpointInstance.Stop()
                    .ConfigureAwait(false);
            }
        }

        private static async Task SendOrderSubmitted(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Sending OrderSubmitted message...");
            await endpointInstance.SendLocal(new OrderSubmitted
                {
                    OrderId = Guid.NewGuid(),
                    Value = DateTime.UtcNow.ToLongTimeString()
                })
                .ConfigureAwait(false);
        }

        private static async Task SendOrderSubmittedWithError(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Sending OrderSubmittedWithError message...");
            await endpointInstance.SendLocal(new OrderSubmittedWithError
                {
                    OrderId = Guid.NewGuid(),
                    Value = DateTime.UtcNow.ToLongTimeString()
                })
                .ConfigureAwait(false);
        }

        private static async Task SendBulkOrderSubmittedWithError(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Sending BulkOrderSubmittedWithError message...");
            await endpointInstance.SendLocal(new BulkOrderSubmittedWithError
                {
                    OrderIds = Enumerable.Range(0, 9).Select(_ => Guid.NewGuid()).ToArray(),
                    Value = DateTime.UtcNow.ToLongTimeString()
                })
                .ConfigureAwait(false);
        }
    }
}