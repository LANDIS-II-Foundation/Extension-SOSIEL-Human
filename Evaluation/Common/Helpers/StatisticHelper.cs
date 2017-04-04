using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Helpers
{
    using Entities;
    using Models;

    public static class StatisticHelper
    {
        public static void SaveState(string outputFolder, string state, AgentList agentList, SiteList siteList)
        {
            Task nodeTask = Task.Factory.StartNew(
                () => agentList.Agents
                .Select(a => new NodeOutput
                {
                    AgentId = a.Id,
                    Type = EnumHelper.EnumValueAsString(a[Agent.VariablesUsedInCode.AgentSubtype])
                })
                .ToArray()
            ).ContinueWith(data =>
            {
                ResultSavingHelper.Save(data.Result, $@"{outputFolder}\nodes_{state}.csv");
            });

            Task edgeTask = Task.Factory.StartNew(
                () => agentList.Agents
                .Where(a => a[Agent.VariablesUsedInCode.AgentCurrentSite] != null)
                .SelectMany(a => siteList.AdjacentSites((Site)a[Agent.VariablesUsedInCode.AgentCurrentSite])
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
                ResultSavingHelper.Save(data.Result, $@"{outputFolder}\edges_{state}.csv");
            });


            Task.WaitAll(nodeTask, edgeTask);
        }




    }
}
