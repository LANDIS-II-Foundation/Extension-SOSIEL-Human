using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Landis.Core;
using Landis.Extension.SOSIELHuman;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            ExtensionMain extension = new PlugIn();

            Console.WriteLine("Loading Parameters");

            string algorithmConfigurationFileName = "configuration.json";
            string algorithmConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), algorithmConfigurationFileName);

            extension.LoadParameters(algorithmConfigurationFilePath, null);

            Console.WriteLine("Initialize");

            extension.Initialize();


            //external cycle
            for(int i=1; i< 2; i++)
            {
                Console.WriteLine(string.Format("Execute iteration {0}", i));

                extension.Run();
            }

                       

            Console.WriteLine("Algorithm has been completed");

            WaitKeyPress();

            Process.Start(Directory.GetCurrentDirectory());
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception;

            if (e.ExceptionObject is AggregateException)
            {
                exception = (e.ExceptionObject as AggregateException).InnerException;
            }
            else
            {
                exception = e.ExceptionObject as Exception;
            }


            Console.WriteLine($"ERROR! {exception.Message}");

            WaitKeyPress();

            Environment.Exit(1);
        }

        private static void WaitKeyPress()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
