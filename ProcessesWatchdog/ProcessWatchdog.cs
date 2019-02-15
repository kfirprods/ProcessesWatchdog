using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ProcessesWatchdog
{
    public delegate void ProcessStatusChanged();

    public class ProcessWatchdog
    {
        // Default sleep time to minimize CPU usage
        private const int DefaultWorkerSleepTime = 1500;

        public event ProcessStatusChanged OnProcessOpened;
        public event ProcessStatusChanged OnProcessClosed;

        private readonly string _processName;
        private readonly int _workerThreadSleepTime;
        private readonly Thread _watchdogWorkerThread;

        // Is the watchdog itself running?
        private bool _isRunning;

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
            this._watchdogWorkerThread = new Thread(ProcessWatchdogWorker) { IsBackground = true };
        }
        
        public void Start()
        {
            if (this._isRunning) return;

            this._isRunning = true;
            this._watchdogWorkerThread.Start();
        }

        public void Stop()
        {
            this._isRunning = false;
            this._watchdogWorkerThread.Join();
        }

        private static bool IsProcessRunning(string processName)
        {
            var runningProcesses = Process.GetProcesses();
            return runningProcesses.Any(process => process.ProcessName == processName);
        }

        private void ProcessWatchdogWorker()
        {
            while (this._isRunning)
            {
                var isProcessOpen = IsProcessRunning(this._processName);

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

                Thread.Sleep(this._workerThreadSleepTime);
            }
        }
    }
}
