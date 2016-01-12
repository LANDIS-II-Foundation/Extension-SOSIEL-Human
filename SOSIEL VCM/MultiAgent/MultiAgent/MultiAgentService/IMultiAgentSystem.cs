using System;
using System.Collections.Generic;

namespace MultiAgent
{
    public interface IMultiAgentSystem<T> where T : IComparable<T>
    {
        List<Agent<T>> Agents { get; }

        /// <summary>
        ///     Initializing Agents here
        /// </summary>
        /// <param name="n">Number of agents</param>
        /// <param name="endowmentParameter">Initial endowment value</param>
        void InititalizeAgents(int n, T endowmentParameter);

        /// <summary>
        ///     Running the main system service
        /// </summary>
        /// <param name="iterations"></param>
        /// <param name="mParameter"></param>
        void RunService(int iterations, double mParameter);

        /// <summary>
        ///     Running the main system service for one iteration
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="mParameter"></param>
        void RunServiceOnce(int iteration, double mParameter);
    }
}