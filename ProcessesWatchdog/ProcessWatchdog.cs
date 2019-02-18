using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace ProcessesWatchdog
{
    public delegate void ProcessStatusChanged();

    public class ProcessWatchdog
    {
        // Default sleep time to minimize CPU usage
        private const int DefaultWorkerSleepTime = 1500;

        // The thread is static because we only want to have one thread polling the system processes
        private static Thread _watchdogWorkerThread;
        private static List<ProcessWatchdog> _registeredWatchdogs;
        private static readonly object RegisteredWatchdogsLock = new object();

        public event ProcessStatusChanged OnProcessOpened;
        public event ProcessStatusChanged OnProcessClosed;

        private readonly string _processName;
        private readonly int _workerThreadSleepTime;
        
        // Is the process currently open?
        private bool _isProcessCurrentlyOpen;

        /// <summary>
        /// Instantiates a watch dog
        /// </summary>
        /// <param name="processName">Process to watch</param>
        /// <param name="workerThreadSleepTime">Milliseconds to sleep between each iteration on system process.
        /// Lower values will increase CPU usage</param>
        public ProcessWatchdog(string processName, int workerThreadSleepTime = DefaultWorkerSleepTime)
        {
            this._processName = processName;
            this._workerThreadSleepTime = workerThreadSleepTime;

            lock (RegisteredWatchdogsLock)
            {
                if (_watchdogWorkerThread == null)
                    _watchdogWorkerThread = new Thread(ProcessWatchdogWorker) {IsBackground = true};

                if (_registeredWatchdogs == null)
                    _registeredWatchdogs = new List<ProcessWatchdog>();
            }
        }
        
        public void Start()
        {
            lock (RegisteredWatchdogsLock)
            {
                _registeredWatchdogs.Add(this);

                if (_watchdogWorkerThread.ThreadState == ThreadState.Stopped)
                    _watchdogWorkerThread = new Thread(ProcessWatchdogWorker) { IsBackground = true };

                if (_watchdogWorkerThread.ThreadState != ThreadState.Background)
                    _watchdogWorkerThread.Start();
            }
        }

        public void Stop()
        {
            lock (RegisteredWatchdogsLock)
            {
                _registeredWatchdogs.Remove(this);

                if (_registeredWatchdogs.Count == 0 && _watchdogWorkerThread.ThreadState == ThreadState.Background)
                    _watchdogWorkerThread.Join();
            }
        }

        private void Update(IEnumerable<Process> runningProcesses)
        {
            var isProcessOpen = runningProcesses.Any(process => process.ProcessName.Equals(this._processName));

            if (isProcessOpen && !this._isProcessCurrentlyOpen)
            {
                this._isProcessCurrentlyOpen = true;
                this.OnProcessOpened?.Invoke();
            }
            else if (!isProcessOpen && _isProcessCurrentlyOpen)
            {
                this._isProcessCurrentlyOpen = false;
                this.OnProcessClosed?.Invoke();
            }
        }

        private static int GetWatchdogsCountSafe()
        {
            lock (RegisteredWatchdogsLock)
            {
                return _registeredWatchdogs.Count;
            }
        }
        
        private static void ProcessWatchdogWorker()
        {
            while (GetWatchdogsCountSafe() > 0)
            {
                int minimalSleepTime;

                lock (RegisteredWatchdogsLock)
                {
                    var runningProcesses = Process.GetProcesses();
                    
                    foreach (var watchdog in _registeredWatchdogs)
                    {
                        watchdog.Update(runningProcesses);
                    }
                    
                    minimalSleepTime = _registeredWatchdogs.Min(watchdog => watchdog._workerThreadSleepTime);
                }

                Thread.Sleep(minimalSleepTime);
            }
        }
    }
}
