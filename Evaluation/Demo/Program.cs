using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Factory;
using System.Threading.Tasks;

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

            if(File.Exists(algorithmConfigurationFilePath) == false)
            {
                throw new FileNotFoundException($"{algorithmConfigurationFileName} not found at {Directory.GetCurrentDirectory()}");
            }

            string jsonContent = File.ReadAllText(algorithmConfigurationFilePath);

            var algorithm = AlgorithmFactory.Create(jsonContent);

            Console.WriteLine($"{algorithm.Name} algorithm is running....");
            
            Task.WaitAll(algorithm.Run());

            Console.WriteLine("Algorithm has completed");

            WaitKeyPress();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;

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
