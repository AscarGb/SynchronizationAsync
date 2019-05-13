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
        ConcurrentQueue<ManualResetEventAwaiter> awaitQueue
            = new ConcurrentQueue<ManualResetEventAwaiter>();

        public bool State { get; private set; }

        public ManualResetEventAsync(bool initialState)
        {
            State = initialState;
        }

        public ManualResetEventAwaiter WaitOneAsync()
        {
            var awaitable = new ManualResetEventAwaiter();

            if (State)
                awaitable.Set();
            else
                awaitQueue.Enqueue(awaitable);

            return awaitable;
        }

        public void Set()
        {
            State = true;

            while (State && awaitQueue.TryDequeue(out var awaitable))
                awaitable.Set();
        }

        public void Reset()
        {
            State = false;
        }
    }

    public class ManualResetEventAwaiter : INotifyCompletion
    {
        Action _continuation;
        public void OnCompleted(Action continuation)
        {
            Volatile.Write(ref _continuation, continuation);
        }

        public bool GetResult() => true;

        public bool IsCompleted { get; private set; } = false;

        public ManualResetEventAwaiter GetAwaiter() => this;

        public void Set()
        {
            IsCompleted = true;

            Action continuation = Interlocked.Exchange(ref _continuation, null);

            if (continuation != null)
                Task.Run(continuation);
        }
    }
}