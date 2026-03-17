using System;

namespace Plugins.MessagePipe.MessageBus.Runtime
{
    public interface IAsyncSignal
    {
        public Guid Id { get; set; } 
    }
}