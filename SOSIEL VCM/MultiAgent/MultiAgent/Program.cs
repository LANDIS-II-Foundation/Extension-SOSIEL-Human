using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace MultiAgent
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Agents number:");
            var n = int.Parse(Console.ReadLine());

            Console.WriteLine("m Number");
            var m = int.Parse(Console.ReadLine());

            Console.WriteLine("Iterations Number");
            var iterations = int.Parse(Console.ReadLine());

            Func<double, double> cooperationFunc = d => { return d >= 5 ? 10 : 0; };
            Func<double, double> trendFunc = d => { return d; };
            Func<double, double> freeRiderFunc = d => { return 0; };

            var multiSystemAgent = new MultiAgentSystem(cooperationFunc, trendFunc, freeRiderFunc);
            multiSystemAgent.InititalizeAgents(n);

            multiSystemAgent.RunService(iterations, m, null);

            Console.Read();
        }
    }
}