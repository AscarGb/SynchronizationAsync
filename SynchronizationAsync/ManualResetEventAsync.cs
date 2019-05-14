using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationAsync
{
    public class ManualResetEventAsync
    {
        ConcurrentQueue<ManualResetEventAwaiter> awaitQueue = null;

        public ManualResetEventAsync(bool initialState)
        {
            if (!initialState)
                Reset();
        }

        public ManualResetEventAwaiter WaitOneAsync()
        {
            var awaitable = new ManualResetEventAwaiter();

            var queue = Volatile.Read(ref awaitQueue);

            if (queue == null)
                awaitable.Continue();
            else
                queue.Enqueue(awaitable);

            return awaitable;
        }

        public void Set()
        {
            var queue = Interlocked.Exchange(ref awaitQueue, null);

            if (queue != null)
                while (queue.TryDequeue(out var awaitable))
                    awaitable.Continue();
        }

        public void Reset()
        {
            Interlocked.CompareExchange(
                ref awaitQueue,
                new ConcurrentQueue<ManualResetEventAwaiter>(),
                null);
        }
    }

    public class ManualResetEventAwaiter : INotifyCompletion
    {
        Action _continuation;
        bool _isCompleted = false;
        public void OnCompleted(Action continuation)
        {
            Volatile.Write(ref _continuation, continuation);
        }

        public bool IsCompleted => _isCompleted;

        public bool GetResult() => true;

        public ManualResetEventAwaiter GetAwaiter() => this;

        public void Continue()
        {
            Volatile.Write(ref _isCompleted, true);

            var continuation = Interlocked.Exchange(ref _continuation, null);

            if (continuation != null)
                Task.Run(continuation);
        }
    }
}