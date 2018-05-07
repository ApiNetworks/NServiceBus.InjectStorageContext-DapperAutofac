using System;
using NServiceBus;

namespace InjectStorageContext.Messages
{
    public class OrderSubmittedWithError : IMessage
    {
        public Guid OrderId { get; set; }
        public string Value { get; set; }
    }
}