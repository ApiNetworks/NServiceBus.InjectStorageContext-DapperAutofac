using System;
using System.Linq;
using System.Threading.Tasks;
using InjectStorageContext.Messages;
using InjectStorageContext.Repositories;
using NServiceBus;
using NServiceBus.Logging;
using Order = InjectStorageContext.Repositories.Order;

namespace InjectStorageContext.Handlers
{
    public class OrderSubmittedHandler : IHandleMessages<OrderSubmitted>, IHandleMessages<OrderSubmittedWithError>, IHandleMessages<BulkOrderSubmittedWithError>
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderSubmittedHandler>();

        private IOrderRepository _orderRepository;

        public OrderSubmittedHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
        {
            Log.Info($"Handling order: {message.OrderId}");

            var order = new Order
            {
                OrderId = message.OrderId,
                Value = message.Value
            };

            await _orderRepository.Add(order).ConfigureAwait(false);
        }

        public async Task Handle(OrderSubmittedWithError message, IMessageHandlerContext context)
        {
            Log.Info($"Handling order with error: {message.OrderId}");

            var order = new Order
            {
                OrderId = message.OrderId,
                Value = message.Value
            };

            await _orderRepository.Add(order).ConfigureAwait(false);
            
            throw new Exception("Boom!");
        }

        public async Task Handle(BulkOrderSubmittedWithError message, IMessageHandlerContext context)
        {
            Log.InfoFormat("Handling bulk order with error: {0}", string.Join(", ", message.OrderIds));

            var orders = message.OrderIds.Select(id => 
                new Order
                {
                    OrderId = id,
                    Value = message.Value
                }).ToList();

            await _orderRepository.Add(orders).ConfigureAwait(false);
            
            throw new Exception("Boom!");
            
        }
    }
}