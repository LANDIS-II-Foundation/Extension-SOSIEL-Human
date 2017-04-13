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
        public static void SaveState(string outputFolder, string state, IAgent[] activeAgents)
        {
            Task nodeTask = Task.Factory.StartNew(
                () => activeAgents
                .Select(a => new NodeOutput
                {
                    AgentId = a.Id,
                    Type = a.ContainsVariable(VariablesUsedInCode.AgentSubtype) ? 
                        (a[VariablesUsedInCode.AgentSubtype].GetType() == typeof(string) ? a[VariablesUsedInCode.AgentSubtype] : EnumHelper.EnumValueAsString(a[VariablesUsedInCode.AgentSubtype]) )
                        : "-"
                })
                .ToArray())
                .ContinueWith(data =>
                {
                    Save(data.Result, $@"{outputFolder}\nodes_{state}.csv");
                });


            Task edgeTask = Task.Factory.StartNew(
                () => activeAgents
                .SelectMany(a => a.ConnectedAgents.Select(a2 => new EdgeOutput
                {
                    AgentId = a.Id,
                    AdjacentAgentId = a2.Id 
                }))
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
            NumericValuesItem[] valueItems = variableNames.Select((v, i) =>
            {
                return new NumericValuesItem { Name = v, Values = activeAgents.OrderBy(a => a.Id).Select(a => a[v]).ToArray(), IsFirstVariable = i == 0, IsLastVariable = i == variableNames.Length - 1 };
            }).ToArray();

            return new AgentNumericValuesOutput { Iteration = iteration, Values = valueItems };
        }

        public static CommonValuesOutput CreateCommonValuesRecord(IAgent[] activeAgents, int iteration, params string[] variableNames)
        {
            ValueItem[] valueItems = variableNames.Select(v =>
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
            }).ToArray();

            return new CommonValuesOutput { Iteration = iteration, Values = valueItems };
        }

        public static SubtypeProportionOutput CreateCommonPoolSubtypeProportionRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            IAgent[] suitableAgents = activeAgents.Where(a => (int)a[VariablesUsedInCode.AgentSubtype] == subtype).ToArray();

            double proportion = 0;

            if (suitableAgents.Length > 0)
            {
                proportion = suitableAgents.Average(a =>
                {
                    if ((int)a[VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[VariablesUsedInCode.CommonPoolSubtupeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[VariablesUsedInCode.CommonPoolSize];
                    }
                });
            }


            return new SubtypeProportionOutput { Iteration = iteration, Proportion = proportion };
        }

        public static SubtypeProportionOutput CreateNeighbourhoodSubtypeProportionRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            IAgent[] suitableAgents = activeAgents.Where(a => (int)a[VariablesUsedInCode.AgentSubtype] == subtype).ToArray();

            double proportion = 0;

            if (suitableAgents.Length > 0)
            {
                proportion = suitableAgents.Average(a =>
                {
                    if ((int)a[VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[VariablesUsedInCode.NeighborhoodSubtypeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[VariablesUsedInCode.NeighborhoodSize];
                    }
                });
            }


            return new SubtypeProportionOutput { Iteration = iteration, Proportion = proportion };
        }

        public static CommonPoolSubtypeFrequencyOutput CreateCommonPoolFrequencyRecord(IAgent[] activeAgents, int iteration, int subtype)
        {
            CommonPoolSubtypeFrequencyOutput sf = new CommonPoolSubtypeFrequencyOutput { Iteration = iteration };

            sf.IntervalFrequency = new int[10];

            activeAgents.Where(a => (int)a[VariablesUsedInCode.AgentSubtype] == subtype).AsParallel()
                .Select(a =>
                {
                    if ((int)a[VariablesUsedInCode.AgentSubtype] == subtype)
                    {
                        return (double)a[VariablesUsedInCode.CommonPoolSubtupeProportion];
                    }
                    else
                    {
                        return a.ConnectedAgents.Count(a2 => (int)a2[VariablesUsedInCode.AgentSubtype] == subtype) / (double)a[VariablesUsedInCode.CommonPoolSize];
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

        public static void SaveInitialConditions(string outputFolder, IList<IAgent> activeAgents, params string[] variables)
        {
            List<InitialAgentVariables> records = activeAgents.Select(a => new InitialAgentVariables
            {
                ID = a.Id,
                VariableItems = variables.OrderBy(v=>v).Select(v => new VariableItem { VariableName = v, Value = a[v] }).ToArray()
            }).ToList();


            Save(records, $@"{outputFolder}\initial_agent_variables.csv");
        }


        public static void Save<T>(IList<T> data, string fileName) where T : class
        {
            DelimitedFileEngine<T> engine = new DelimitedFileEngine<T>();

            if (data.Count > 0)
            {
                IHeader first = data.First() as IHeader;

                if (first != null)
                {
                    engine.HeaderText = first.HeaderLine;
                }

            }

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

            aw.Avgs = activeAgents.GroupBy(a => a[VariablesUsedInCode.AgentSubtype])
                .OrderBy(g => g.Key)
                .Select(g => new AvgWellbeingItem { Type = EnumHelper.EnumValueAsString(g.Key), AvgValue = g.Average(a => (double)a[VariablesUsedInCode.AgentSiteWellbeing]) }).ToArray();

            return aw;
        }


        public static CommonPoolSubtypeFrequencyWithDisturbanceOutput CreateCommonPoolFrequencyWithDisturbanceRecord(IAgent[] activeAgents, int iteration, int subtype, double disturbance)
        {
            CommonPoolSubtypeFrequencyWithDisturbanceOutput record = (CommonPoolSubtypeFrequencyWithDisturbanceOutput)CreateCommonPoolFrequencyRecord(activeAgents, iteration, subtype);

            record.Disturbance = disturbance;

            return record;
        }

        public static DebugAgentsPositionOutput CreateDebugAgentsPositionRecord(SiteList siteList, int iteration)
        {
            return new DebugAgentsPositionOutput { Positions = $"{iteration} iteration;{Environment.NewLine}{string.Join(Environment.NewLine, siteList.Sites.Select(l => string.Join(";", l.Select(s => s.IsOccupied ? $"Id:{s.OccupiedBy.Id}, {EnumHelper.EnumValueAsString(s.OccupiedBy[VariablesUsedInCode.AgentSubtype])}" : ""))).ToArray())}{Environment.NewLine}" };
        }
    }
}
