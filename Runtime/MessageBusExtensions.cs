using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

namespace Plugins.MessagePipe.MessageBus.Runtime
{
    public static class MessageBusExtensions
    {
        private static readonly ConcurrentDictionary<Guid, UniTaskCompletionSource<bool>> PendingSignals = new();
        private static readonly ConcurrentDictionary<Guid, Action<IAsyncSignal>> ReleaseActions = new();

        public static void Subscribe<T>(this IMessageDisposable disposable, MessageBus messageBus, Action<T> action)
        {
            messageBus.Subscribe(action, disposable);
        }

        public static Action<T> SubscribeAsync<T>(this IMessageDisposable disposable, MessageBus messageBus, Action<T> action) where T : IAsyncSignal
        {
            messageBus.Subscribe(action, disposable);
            
            Action<T> releaseAction = signal =>
            {
                if (PendingSignals.TryRemove(signal.Id, out var tcs))
                {
                    tcs.TrySetResult(true);
                }
            };

            return releaseAction;
        }

        public static async UniTask PublishAsync<T>(this IMessageDisposable disposable, MessageBus messageBus, T signal) where T : IAsyncSignal
        {
            var id = signal.Id;
            if (id == Guid.Empty)
                throw new InvalidOperationException("Signal Id cannot be empty.");

            var tcs = new UniTaskCompletionSource<bool>();

            if (!PendingSignals.TryAdd(id, tcs))
                throw new InvalidOperationException($"A signal with Id {id} is already being awaited.");

            try
            {
                messageBus.Publish(signal);

                await tcs.Task;
            }
            finally
            {
                PendingSignals.TryRemove(id, out _);
            }
        }
    }
}