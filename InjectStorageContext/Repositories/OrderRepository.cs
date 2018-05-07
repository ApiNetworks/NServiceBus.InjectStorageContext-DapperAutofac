using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using InjectStorageContext.Pipeline;
using NServiceBus.Logging;

namespace InjectStorageContext.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private static readonly ILog Log = LogManager.GetLogger<OrderRepository>();
        private readonly StorageContext _storageContext;

        public OrderRepository(StorageContext storageContext)
        {
            _storageContext = storageContext;
        }

        private IDbConnection Connection => _storageContext.Connection;
        private IDbTransaction Transaction => _storageContext.Transaction;
        
        public async Task Add(Order order)
        {
            Log.Info("Inserting order...");

            await Connection.InsertAsync(order, Transaction);
        }

        public async Task Add(IEnumerable<Order> orders)
        {
            Log.Info("Inserting multiple orders...");

            foreach (var order in orders)
            {
                await Connection.InsertAsync(order, Transaction);
            }
        }
    }
}