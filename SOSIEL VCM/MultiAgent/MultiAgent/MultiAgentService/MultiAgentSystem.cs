using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace MultiAgent
{
    /// <summary>
    ///     Simple implementation of MultiAgentSystem
    /// </summary>
    public class MultiAgentSystem : IMultiAgentSystem<double>
    {
        private readonly IUnityContainer _unityContainer = new UnityContainer();

        /// <summary>
        ///     Initializing StrategyModifier & Calculator services here
        /// </summary>
        /// <param name="cooperationFunc"></param>
        /// <param name="trendFunc"></param>
        /// <param name="freeRiderFunc"></param>
        public MultiAgentSystem(Func<double, double> cooperationFunc, Func<double, double> trendFunc,
            Func<double, double> freeRiderFunc)
        {
            var strategyModifier = new StrategyModifier<double>(cooperationFunc, trendFunc, freeRiderFunc);
            var calculatorService = new CalculatorService();
            _unityContainer.RegisterInstance(strategyModifier);
            _unityContainer.RegisterInstance(calculatorService);
        }

        public List<Agent<double>> Agents { get; protected set; }

        /// <summary>
        ///     Initializing Agents here
        /// </summary>
        /// <param name="n"></param>
        /// <param name="endowmentParameter"></param>
        public void InititalizeAgents(int n, double endowmentParameter)
        {
            Agents = new List<Agent<double>>();
            for (var i = 0; i < n; i++)
            {
                Agents.Add(new Agent<double>
                {
                    Name = "Agent" + i,
                    Strategy = new Strategy<double> { IfCondition = 5, ThenCondition = 10 },
                    Endowment = endowmentParameter,
                    LastPayoff = 0,
                    AgentType = AgentType.FreeRider
                });
            }
        }

        /// <summary>
        ///     Running the main system service
        /// </summary>
        /// <param name="iterations"></param>
        /// <param name="mParameter"></param>
        /// <param name="updatingStatus"></param>
        public void RunService(int iterations, double mParameter)
        {
            for (var i = 0; i < iterations; i++)
            {
               RunServiceOnce(i, mParameter);
            }
        }

        /// <summary>
        ///     Running the main system service for one iteration
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="mParameter"></param>
        /// <param name="updatingStatus"></param>
        public void RunServiceOnce(int iteration, double mParameter)
        {
           var contributions =
                Agents.Select(x => x.IsContributionValid() ? x.Strategy.ThenCondition : x.Strategy.ElseCondition)
                    .ToArray();
            foreach (var agent in Agents)
            {
                agent.Update(
                    _unityContainer.Resolve<CalculatorService>().CalculateNewPayoff(agent.Strategy.ThenCondition,
                        agent.Endowment, contributions, mParameter), contributions.Sum() / Agents.Count);

                agent.Endowment += agent.LastPayoff;

                agent.Contributions.Add(agent.Strategy.ThenCondition);

                agent.Strategy = _unityContainer.Resolve<StrategyModifier<double>>()
                    .Execute(agent.AgentType, contributions.Sum() / Agents.Count, 0);
            }
        }
    }
}