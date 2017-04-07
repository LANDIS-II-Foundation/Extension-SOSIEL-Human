using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Helpers
{
    using Entities;
    using Models;

    public static class StatisticHelper
    {
        public static void SaveState(string outputFolder, string state, IAgent[] activeAgents, SiteList siteList)
        {
            Task nodeTask = Task.Factory.StartNew(
                () => activeAgents
                .Select(a => new NodeOutput
                {
                    AgentId = a.Id,
                    Type = EnumHelper.EnumValueAsString(a[Agent.VariablesUsedInCode.AgentSubtype])
                })
                .ToArray())
                .ContinueWith(data =>
                {
                    Save(data.Result, $@"{outputFolder}\nodes_{state}.csv");
                });

            Task edgeTask = Task.Factory.StartNew(
                () => activeAgents
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
                    Save(data.Result, $@"{outputFolder}\edges_{state}.csv");
                });


            Task.WaitAll(nodeTask, edgeTask);
        }

        public static AgentNumericValuesOutput CreateAgentValuesRecord(IAgent[] activeAgents, int iteration, params string[] variableNames)
        {
            List<NumericValuesItem> valueItems = variableNames.Select(v =>
            {
                return new NumericValuesItem { Name = v, Values = activeAgents.Select(a => a[v]).ToArray() };
            }).ToList();

            return new AgentNumericValuesOutput { Iteration = iteration, Values = valueItems };
        }

        public static CommonValuesOutput CreateCommonValuesRecord(IAgent[] activeAgents, int iteration, params string[] variableNames)
        {
            List<ValueItem> valueItems = variableNames.Select(v =>
            {
                double value = 0;

                if (v.Contains("AVG_"))
                {
                    string varName = v.Replace("AVG_", "");

                    value = activeAgents.Average(a => (double)a[varName]);
                }
                else
                {
                    value = activeAgents.First()[v];
                }

                return new ValueItem { Name = v, Value = value };
            }).ToList();

            return new CommonValuesOutput { Iteration = iteration, Values = valueItems };
        }

        public static SubtypeProportionOutput CreateCommonPoolSubtypeProportionRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            IAgent[] suitableAgents = activeAgents.Where(a => (int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype).ToArray();

            double proportion = 0;

            if (suitableAgents.Length > 0)
            {
                proportion = suitableAgents.Average(a =>
                {
                    if ((int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[Agent.VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[Agent.VariablesUsedInCode.CommonPoolSize];
                    }
                });
            }


            return new SubtypeProportionOutput { Iteration = iteration, Proportion = proportion };
        }

        public static SubtypeProportionOutput CreateNeighbourhoodSubtypeProportionRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            IAgent[] suitableAgents = activeAgents.Where(a => (int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype).ToArray();

            double proportion = 0;

            if (suitableAgents.Length > 0)
            {
                proportion = suitableAgents.Average(a =>
                {
                    if ((int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[Agent.VariablesUsedInCode.NeighborhoodSubtypeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[Agent.VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[Agent.VariablesUsedInCode.NeighborhoodSize];
                    }
                });
            }


            return new SubtypeProportionOutput { Iteration = iteration, Proportion = proportion };
        }

        public static CommonPoolSubtypeFrequencyOutput CreateCommonPoolFrequencyRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            CommonPoolSubtypeFrequencyOutput sf = new CommonPoolSubtypeFrequencyOutput { Iteration = iteration };

            sf.IntervalFrequency = new int[10];

            activeAgents.AsParallel()
                .Select(a =>
                {
                    if ((int)a[Agent.VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[Agent.VariablesUsedInCode.CommonPoolSubtupeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[Agent.VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[Agent.VariablesUsedInCode.CommonPoolSize];
                    }
                })
                .Select(v => new { Interval = Math.Round(v, 1, MidpointRounding.AwayFromZero), Value = v }).AsSequential()
                .ForEach(o =>
                {
                    int i = Convert.ToInt32((o.Interval == 0 ? 0.1 : o.Interval) / 0.1) - 1;

                    sf.IntervalFrequency[i]++;
                });

            return sf;
        }



        public static void Save<T>(IEnumerable<T> data, string fileName) where T : class
        {
            DelimitedFileEngine<T> engine = new DelimitedFileEngine<T>();

            engine.WriteFile(fileName, data);
        }


        public static RuleFrequenciesOutput CreateRuleFrequenciesRecord(IAgent[] activeAgents, int iteration)
        {
            return new RuleFrequenciesOutput
            {
                Iteration = iteration,
                RuleFrequencies = activeAgents.First().MentalModelRules.Select(r => new RuleFrequenceItem
                    { RuleId = r.Id, Frequence = activeAgents.Count(a => a.AssignedRules.Contains(r)) }).ToArray()
            };
        }

        public static AvgWellbeingOutput CreateAvgWellbeingStatisticRecord(IAgent[] activeAgents, int iteration)
        {
            AvgWellbeingOutput aw = new AvgWellbeingOutput { Iteration = iteration };

            aw.Avgs = activeAgents.GroupBy(a => a[Agent.VariablesUsedInCode.AgentSubtype])
                .OrderBy(g => g.Key)
                .Select(g => new AvgWellbeingItem { Type = EnumHelper.EnumValueAsString(g.Key), AvgValue = g.Average(a => a[Agent.VariablesUsedInCode.AgentSiteWellbeing]) }).ToArray();

            return aw;
        }


        public static CommonPoolSubtypeFrequencyWithDisturbanceOutput CreateCommonPoolFrequencyWithDisturbanceRecord(IAgent[] activeAgents, int iteration, int subtype, int disturbance)
        {
            CommonPoolSubtypeFrequencyWithDisturbanceOutput record = (CommonPoolSubtypeFrequencyWithDisturbanceOutput)CreateCommonPoolFrequencyRecord(activeAgents, iteration, subtype);

            record.Disturbance = disturbance;

            return record;
        }
    }
}
