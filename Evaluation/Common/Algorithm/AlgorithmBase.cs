using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Algorithm
{
    using Entities;
    using Helpers;
    using Models;

    public abstract class AlgorithmBase
    {
        protected string _outputFolder;

        protected SiteList _siteList;
        protected AgentList _agentList;


        protected List<SubtypeProportionOutput> _subtypeProportionStatistic;



        public async Task<string> Run()
        {
            Initialize();

            await SaveState("initial");

            ExecuteAlgorithm();

            await SaveState("final");

            SaveProportionStatistic();

            SaveCustomStatistic();

            return _outputFolder;
        }

        protected abstract void Initialize();

        protected abstract void ExecuteAlgorithm();

        protected virtual void SaveCustomStatistic() { }

        protected async Task SaveState(string state)
        {
            Task nodeTask = Task.Factory.StartNew(
                () => _agentList.Agents
                .Select(a => new NodeOutput
                {
                    AgentId = a.Id,
                    Type = EnumHelper.EnumValueAsString(a[Agent.VariablesUsedInCode.AgentSubtype])
                })
                .ToArray()
            ).ContinueWith(data =>
            {
                ResultSavingHelper.Save(data.Result, $@"{_outputFolder}\nodes_{state}.csv");
            });

            Task edgeTask = Task.Factory.StartNew(
                () => _agentList.Agents
                .Where(a => a[Agent.VariablesUsedInCode.AgentCurrentSite] != null)
                .SelectMany(a => _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentCurrentSite])
                .Where(s => s.IsOccupied)
                .Select(s => new EdgeOutput
                {
                    AgentId = a.Id,
                    AdjacentAgentId = s.OccupiedBy.Id
                }
                ))
                .Distinct(new EdgeOutputComparer())
                .ToArray())
            .ContinueWith(data =>
            {
                ResultSavingHelper.Save(data.Result, $@"{_outputFolder}\edges_{state}.csv");
            });

            await nodeTask;
            await edgeTask;
        }


        protected void SaveProportionStatistic()
        {
            ResultSavingHelper.Save(_subtypeProportionStatistic, $@"{_outputFolder}\subtype_proportion.csv");
        }



        //This implementation suitable for models with common pool
        protected virtual double CalculateSubtypeProportion(int subtype, Site centerSite)
        {
            var occupiedCommonPool = _siteList.CommonPool(centerSite).Where(s => s.IsOccupied).ToArray();


            return occupiedCommonPool.Count(s => (int)s.OccupiedBy[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
                / (double)occupiedCommonPool.Length;
        }






        protected SubtypeProportionOutput CreateNeighborhoodSubtypeProportionRecord(int iteration, int subtype)
        {
            SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

            sp.Proportion = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active").AsParallel()
                .Average(a => (double)CalculateSubtypeProportion(subtype, a[Agent.VariablesUsedInCode.AgentCurrentSite]));

            return sp;
        }


        protected SubtypeProportionOutput CreateCommonPoolSubtypeProportionRecord(int iteration, int subtype)
        {
            SubtypeProportionOutput sp = new SubtypeProportionOutput { Iteration = iteration };

            //int temp = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active" && (int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype).Count();

            IAgent[] agents = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active" && (int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype).ToArray();
            
            sp.Proportion = agents.Length > 0 ? agents.Average(a => (double)CalculateSubtypeProportion(subtype, a[Agent.VariablesUsedInCode.AgentCurrentSite]))
                : 0;

            return sp;
        }

        protected virtual CommonPoolSubtypeFrequencyOutput CreateCommonPoolFrequencyRecord(int iteration, double disturbance, int subtype)
        {
            CommonPoolSubtypeFrequencyOutput sf = new CommonPoolSubtypeFrequencyOutput { Iteration = iteration, Disturbance = disturbance };

            sf.IntervalFrequency = new int[10];

            _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active").AsParallel()
                .Select(a => CalculateSubtypeProportion(subtype, a[Agent.VariablesUsedInCode.AgentCurrentSite]))
                .Select(v => new { Interval = Math.Round(v, 1, MidpointRounding.AwayFromZero), Value = v }).AsSequential()
                .ForEach(o =>
                {
                    int i = Convert.ToInt32((o.Interval == 0 ? 0.1 : o.Interval) / 0.1) - 1;

                    sf.IntervalFrequency[i]++;
                });

            return sf;
        }

        protected virtual double CalculateAgentWellbeing(IAgent agent, Site centerSite)
        {
            throw new NotImplementedException($"Method CalculateAgentWellbeing not implemented for agent {agent.GetType().Name}");
        }

        protected virtual void FindInactiveAgents()
        {
            _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active").AsParallel()
                .Select(agent => new
                {
                    agent,
                    Wellbeing = CalculateAgentWellbeing(agent, agent[Agent.VariablesUsedInCode.AgentCurrentSite])
                })
                .Where(obj => obj.Wellbeing <= 0).ToArray()
                .ForEach(obj =>
                {
                    obj.agent[Agent.VariablesUsedInCode.AgentStatus] = "inactive";
                    obj.agent[Agent.VariablesUsedInCode.AgentCurrentSite] = null;
                });
        }

        protected virtual void Reproduction<T>(int minAgentNumber, int noncoType) where T : class, IAgent, ICloneableAgent<T>
        {
            List<IAgent> activeAgents = _agentList.Agents.Where(a => a[Agent.VariablesUsedInCode.AgentStatus] == "active" 
                && _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentCurrentSite])
                    .Where(s => s.IsOccupied).Sum(s=>s.OccupiedBy[Agent.VariablesUsedInCode.AgentC]) > 0).ToList();

            if (activeAgents.Count < 1)
                return;

            int newAgentCount = minAgentNumber - activeAgents.Count;

            int lastAgentId = _agentList.Agents.Count + 1;

            while (newAgentCount > 0)
            {
                IAgent targetAgent = activeAgents.RandomizeOne();

                IAgent[] poolOfParticipants = _siteList.CommonPool((Site)targetAgent[Agent.VariablesUsedInCode.AgentCurrentSite])
                    .Where(s => s.IsOccupied).Select(s => s.OccupiedBy).ToArray();

                int contributionsAmount = poolOfParticipants.Sum(a => (int)a[Agent.VariablesUsedInCode.AgentC]);

                List<IAgent> vector = new List<IAgent>(100);

                poolOfParticipants.ForEach(a =>
                {
                    int count = Convert.ToInt32(Math.Round(a[Agent.VariablesUsedInCode.AgentC] / (double)contributionsAmount * 100, MidpointRounding.AwayFromZero));

                    for (int i = 0; i < count; i++) { vector.Add(a); }
                });

                T prototype = vector.RandomizeOne() as T;

                T replica = prototype.Clone();

                replica.Id = lastAgentId;

                lastAgentId++;

                Site targetSite = _siteList.TakeClosestEmptySites((Site)targetAgent[Agent.VariablesUsedInCode.AgentCurrentSite]).RandomizeOne();

                replica[Agent.VariablesUsedInCode.AgentCurrentSite] = targetSite;

                newAgentCount--;

                _agentList.Agents.Add(replica);
                activeAgents.Add(replica);
            }
        }
    }
}
