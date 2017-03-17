using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;


using Factory;


namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Console.WriteLine("Reading configuration");

            string algorithmConfigurationFileName = "algorithm.json";
            string algorithmConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), algorithmConfigurationFileName);

            if (File.Exists(algorithmConfigurationFilePath) == false)
            {
                throw new FileNotFoundException($"{algorithmConfigurationFileName} not found at {Directory.GetCurrentDirectory()}");
            }

            string jsonContent = File.ReadAllText(algorithmConfigurationFilePath);

            var algorithm = AlgorithmFactory.Create(jsonContent);

            Console.WriteLine($"{algorithm.Name} algorithm is running....");

            string outputDirectory= string.Empty;

            Task.WaitAll(algorithm.Run().ContinueWith(d=> outputDirectory = d.Result));

            Console.WriteLine("Algorithm has completed");

            WaitKeyPress();

            Process.Start(Path.Combine(Directory.GetCurrentDirectory(), outputDirectory));
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception[] exceptions;

            if (e.ExceptionObject is AggregateException)
            {
                exceptions = (e.ExceptionObject as AggregateException).InnerExceptions.ToArray();
            }
            else
            {
                exceptions = new Exception[] { e.ExceptionObject as Exception };
            }

            foreach(var ex in exceptions)
                Console.WriteLine($"ERROR! {ex.Message}");

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
