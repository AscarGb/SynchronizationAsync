using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationAsync
{
    public class ManualResetEventAsyncTCS
    {
        private TaskCompletionSource<bool> tcs = null;

        public ManualResetEventAsyncTCS(bool initialState)
        {
            if (!initialState)
                Reset();
        }

        public Task WaitOneAsync()
        {
            var tcs = Volatile.Read(ref this.tcs);
            if (tcs == null)
                return Task.CompletedTask;
            else
                return tcs.Task;
        }

        public void Set()
        {
            var old_tcs = Interlocked.Exchange(ref this.tcs, null);
            old_tcs?.SetResult(false);
        }

        public void Reset()
        {
            var new_tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Interlocked.CompareExchange(ref this.tcs, new_tcs, null);
        }
    }
}
