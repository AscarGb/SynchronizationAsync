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
        ConcurrentQueue<ManualResetEventAwaitable> awaiteQueue
            = new ConcurrentQueue<ManualResetEventAwaitable>();

        public bool State { get; private set; }

        public ManualResetEventAsync(bool initialState)
        {
            State = initialState;
        }

        public ManualResetEventAwaitable WaitOneAsync()
        {
            var awaitable = new ManualResetEventAwaitable();

            if (State)
                awaitable.Set();
            else
                awaiteQueue.Enqueue(awaitable);

            return awaitable;
        }

        public void Set()
        {
            lock (awaiteQueue)
            {
                while (awaiteQueue.TryDequeue(out var awaitable))
                    awaitable.Set();

                State = true;
            }
        }

        public void Reset()
        {
            State = false;
        }
    }

    public class ManualResetEventAwaitable
    {
        ManualResetEventAwaiter _manualResetEventAwaiter;
        public ManualResetEventAwaitable()
        {
            _manualResetEventAwaiter = new ManualResetEventAwaiter();
        }
        public ManualResetEventAwaiter GetAwaiter()
        {
            return _manualResetEventAwaiter;
        }
        public void Set()
        {
            _manualResetEventAwaiter.Set();
        }
    }

    public class ManualResetEventAwaiter : INotifyCompletion
    {
        Action _continuation;
        public void OnCompleted(Action continuation)
        {
            Volatile.Write(ref _continuation, continuation);
        }

        public bool GetResult()
        {
            return true;
        }

        public bool IsCompleted { get; private set; } = false;

        public void Set()
        {
            IsCompleted = true;

            Action continuation = Interlocked.Exchange(ref _continuation, null);

            Task.Run(() =>
            {
                continuation?.Invoke();
            });
        }
    }
}