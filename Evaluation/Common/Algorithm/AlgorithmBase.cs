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



        public async Task Run()
        {
            Initialize();

            await SaveState("initial");

            ExecuteAlgorithm();

            await SaveState("final");

            SaveProportionStatistic();

            SaveCustomStatistic();
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
                .SelectMany(a => _siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentSite])
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
            ResultSavingHelper.Save(_subtypeProportionStatistic, $@"{_outputFolder}\subtype_A_proportion.csv");
        }

    }
}
