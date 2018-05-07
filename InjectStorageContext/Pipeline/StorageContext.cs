using System;
using System.Data;
using NServiceBus.Logging;

namespace InjectStorageContext.Pipeline
{
    public class StorageContext : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger<StorageContext>();

        private static long _count;
        private readonly long _instance = ++_count;

        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }

        public StorageContext()
        {
            Log.Info($"Created: {_instance}");
        }

        public void Dispose()
        {
            Log.Info($"Dispose: {_instance}");
        }
    }
}