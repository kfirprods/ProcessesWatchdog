﻿using System;
using System.Linq;
using ProcessesWatchdog;

namespace ProcessesWatchdogSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create multiple watch dogs for various processes
            var watchedProcessNames = new[] {"Calculator", "mspaint", "Steam", "Spotify"};
            var watchdogs = watchedProcessNames.Select(CreateLoggingWatchDog).ToArray();
            
            foreach (var watchdog in watchdogs)
            {
                watchdog.Start();
            }

            Console.WriteLine("Press any key to stop the watch dogs");
            Console.ReadKey();

            Console.WriteLine("Stopping the watchdogs...");

            foreach (var watchdog in watchdogs)
            {
                watchdog.Stop();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            
        }

        /// <summary>
        /// Creates a watch dog that writes to console when the process is started or terminated
        /// </summary>
        private static ProcessWatchdog CreateLoggingWatchDog(string processName)
        {
            var watchdog = new ProcessWatchdog(processName);
            watchdog.OnProcessOpened += (pid) => { Console.WriteLine($"{processName} is running with PID {pid}"); };
            watchdog.OnProcessClosed += () => { Console.WriteLine($"{processName} is terminated"); };

            return watchdog;
        }
    }
}
