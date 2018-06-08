// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;

namespace Steeltoe.Discovery.Consul.Internal
{
    internal class TimedTask : IDisposable
    {
        private readonly string _name;
        private readonly Action _task;
        private readonly Timer _timer;
        private int _taskRunning;

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