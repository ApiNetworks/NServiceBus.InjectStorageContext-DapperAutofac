using System;
using NServiceBus;

namespace InjectStorageContext.Messages
{
    public class BulkOrderSubmittedWithError : IMessage
    {
        public Guid[] OrderIds { get; set; }
        public string Value { get; set; }
    }
}