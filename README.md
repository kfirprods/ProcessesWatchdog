# ProcessesWatchdog
A C# library to monitor the opening and closing of specific Windows processes.

Get it on NuGET: https://www.nuget.org/packages/ProcessesWatchdog


Example (available under ProcessesWatchdogSample):

```C#
using System;
using ProcessesWatchdog;

namespace ProcessesWatchdogSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Watch for the windows calculator process
            var watchdog = new ProcessWatchdog("Calculator");
            watchdog.OnProcessOpened += () => { Console.WriteLine("Calculator.exe is running"); };
            watchdog.OnProcessClosed += () => { Console.WriteLine("Calculator.exe is terminated"); };
            watchdog.Start();

            Console.WriteLine("Press any key to stop the watch dog");
            Console.ReadKey();

            Console.WriteLine("Stopping the watchdog...");
            watchdog.Stop();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
```
