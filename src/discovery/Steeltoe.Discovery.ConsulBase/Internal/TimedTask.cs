using System;
using System.Threading;

namespace Steeltoe.Discovery.Consul.Internal
{
    internal class TimedTask : IDisposable
    {
        private readonly string _name;
        private readonly Action _task;
        private int _taskRunning;
        private readonly Timer _timer;

        public TimedTask(string name, Action task, int interval)
        {
            _name = name;
            _task = task;
            _timer = new Timer(Run, null, interval, interval);
        }

        public void Run(object state)
        {
            if (Interlocked.CompareExchange(ref _taskRunning, 1, 0) == 0)
            {
                try
                {
                    _task();
                }
                catch (Exception)
                {
                    // Log
                }

                Interlocked.Exchange(ref _taskRunning, 0);
            }
            else
            {
                // Log, already running
            }
        }

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        #endregion IDisposable
    }
}