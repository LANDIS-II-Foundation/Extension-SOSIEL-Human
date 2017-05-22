using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using Landis.Core;
using Landis.Extension.SOSIELHuman;

using Landis.Extension.SOSIELHuman.Output;
using Landis.Extension.SOSIELHuman.Helpers;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            HMRuleUsageOutput ruo = new HMRuleUsageOutput { Iteration = 1, ActivatedRules = new string[] { "RS1_L1_R1", "RS1_L1_R2" }, NotActivatedRules = new string[] { "RS1_L1_R3", "RS1_L1_R4", "RS1_L1_R5" } };

            FEValuesOutput vo = new FEValuesOutput { Iteration = 1, Biomass = 100, ReductionPercentage = 10, BiomassReduction = 10.990000, Profit = 10, Site = "S1" };

            WriteToCSVHelper.AppendTo("ruo.csv", ruo);
            WriteToCSVHelper.AppendTo("vo.csv", vo);


            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            PlugIn extension = new PlugIn();





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


            Console.WriteLine(string.Format("ERROR! {0}", exception.Message));

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
