﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace MultiAgent
{
    /// <summary>
    ///     Simple implementation of MultiAgentSystem
    /// </summary>
    public class MultiAgentSystem
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
        public void InititalizeAgents(int n)
        {
            Agents = new List<Agent<double>>();
            for (var i = 0; i < n; i++)
            {
                Agents.Add(new Agent<double>
                {
                    Name = "Agent" + i,
                    Strategy = new Strategy<double> {IfCondition = 5, ThenCondition = 10},
                    Endowment = 10,
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
        public void RunService(int iterations, double mParameter, Action<string> updatingStatus)
        {
            for (var i = 0; i < iterations; i++)
            {
                if (updatingStatus != null)
                    updatingStatus("Iteration...." + (i + 1));

                var contributions =
                    Agents.Select(x => x.IsContributionValid() ? x.Strategy.ThenCondition : x.Strategy.ElseCondition)
                        .ToArray();
                foreach (var agent in Agents)
                {
                    agent.Update(
                        _unityContainer.Resolve<CalculatorService>().CalculateNewPayoff(agent.Strategy.ThenCondition,
                            agent.Endowment, contributions, mParameter), contributions.Sum()/Agents.Count);

                    agent.Endowment += agent.LastPayoff;

                    agent.Contributions.Add(agent.Strategy.ThenCondition);

                    if (updatingStatus != null)
                        updatingStatus(agent.Name);
                    if (updatingStatus != null)
                        updatingStatus(agent.LastPayoff.ToString());
                    if (updatingStatus != null)
                        updatingStatus(agent.Endowment.ToString());

                    if (updatingStatus != null)
                        updatingStatus("choosing new strategy...." + agent.Name);

                    agent.Strategy = _unityContainer.Resolve<StrategyModifier<double>>()
                        .Execute(agent.AgentType, contributions.Sum()/Agents.Count, 0);
                }
            }
        }
    }
}