using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.SOSIELHuman.Processes
{
    using Entities;
    using Helpers;
    using Enums;


    public class ActionSelection : VolatileProcess
    {
        Goal processedGoal;
        GoalState goalState;


        AgentState agentState;

        Rule[] matchedRules;


        Rule priorPeriodActivatedRule;
        Rule ruleForActivating;

        protected override void AboveMin()
        {
            Rule[] selected = new Rule[] { };

            var ai = agentState.AnticipationInfluence;

            if (goalState.DiffCurrentAndFocal > 0)
            {
                if (matchedRules.Any(r => r == priorPeriodActivatedRule))
                {
                    ruleForActivating = priorPeriodActivatedRule;
                    return;
                }
                else
                {
                    selected = matchedRules.Where(r => ai[r][processedGoal] >= 0 &&
                        ai[r][processedGoal] < goalState.DiffCurrentAndFocal).ToArray();
                }
            }
            else
            {
                selected = matchedRules.Where(r => ai[r][processedGoal] >= 0 &&
                    ai[r][processedGoal] > goalState.DiffCurrentAndFocal).ToArray();
            }

            if (selected.Length > 0)
            {
                selected = selected.GroupBy(r => ai[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                ruleForActivating = selected.RandomizeOne();
            }
        }

        protected override void BelowMax()
        {
            Rule[] selected = new Rule[] { };

            var ai = agentState.AnticipationInfluence;

            if (goalState.DiffCurrentAndFocal < 0)
            {
                if (matchedRules.Any(r => r == priorPeriodActivatedRule))
                {
                    ruleForActivating = priorPeriodActivatedRule;
                    return;
                }
                else
                {
                    selected = matchedRules.Where(r => ai[r][processedGoal] <= 0 &&
                        Math.Abs(ai[r][processedGoal]) < Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
                }
            }
            else
            {
                selected = matchedRules.Where(r => ai[r][processedGoal] < 0 &&
                    Math.Abs(ai[r][processedGoal]) > Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
            }

            if (selected.Length > 0)
            {
                selected = selected.GroupBy(r => ai[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                ruleForActivating = selected.RandomizeOne();
            }
        }

        protected override void Maximize()
        {
            var ai = agentState.AnticipationInfluence;

            if (matchedRules.Length > 0)
            {
                Rule[] selected = matchedRules.GroupBy(r => ai[r][processedGoal]).OrderByDescending(hg => hg.Key).First().ToArray();

                ruleForActivating = selected.RandomizeOne();
            }
        }

        void ShareCollectiveAction(IAgent currentAgent, Rule rule, Dictionary<IAgent, AgentState> agentStates)
        {
            foreach (IAgent agent in currentAgent.ConnectedAgents)
            {
                if (agent.AssignedRules.Contains(rule) == false)
                {
                    agent.AssignNewRule(rule);

                    if (agentStates[agent].AnticipationInfluence.ContainsKey(rule))
                    {
                        agentStates[agent].AnticipationInfluence[rule] = new Dictionary<Goal, double>(agentStates[currentAgent].AnticipationInfluence[rule]);

                    }
                    else
                    {
                        agentStates[agent].AnticipationInfluence.Add(rule, new Dictionary<Goal, double>(agentStates[currentAgent].AnticipationInfluence[rule]));
                    }
                }
            }
        }

        public void ExecutePartI(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> iterationState,
            Goal[] rankedGoals, Rule[] processedRules)
        {
            ruleForActivating = null;

            agentState = iterationState.Value[agent];
            AgentState priorPeriod = iterationState.Previous?.Value[agent];



            processedGoal = rankedGoals.First(g => processedRules.First().Layer.Set.AssociatedWith.Contains(g));
            goalState = agentState.GoalsState[processedGoal];

            matchedRules = processedRules.Except(agentState.BlockedRules).Where(h => h.IsMatch(agent)).ToArray();

            if (matchedRules.Length > 1)
            {
                priorPeriodActivatedRule = priorPeriod.Activated.FirstOrDefault(r => r.Layer == processedRules.First().Layer);

                SpecificLogic(processedGoal.Tendency);

                //if none are identified, then choose the do-nothing heuristic.

                //todo
                if (ruleForActivating == null)
                {
                    ruleForActivating = processedRules.Single(h => h.IsAction == false);
                }
            }
            else
                ruleForActivating = matchedRules[0];

            if (processedRules.First().Layer.Set.Layers.Count > 1)
                ruleForActivating.Apply(agent);


            if (ruleForActivating.IsCollectiveAction)
            {
                ShareCollectiveAction(agent, ruleForActivating, iterationState.Value);
            }


            agentState.Activated.Add(ruleForActivating);
            agentState.Matched.AddRange(matchedRules);
        }

        public void ExecutePartII(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> iterationState,
            Goal[] rankedGoals, Rule[] processedRules, IAgent[] activeAgents)
        {
            AgentState agentState = iterationState.Value[agent];

            RuleLayer layer = processedRules.First().Layer;

            Rule selectedRule = agentState.Activated.Single(r => r.Layer == layer);

            if (selectedRule.IsCollectiveAction)
            {
                int numberOfInvolvedAgents = 0;

                int requiredParticipants = selectedRule.RequiredParticipants;


                if (numberOfInvolvedAgents < requiredParticipants)
                {
                    agentState.BlockedRules.Add(selectedRule);

                    agentState.Activated.Remove(selectedRule);

                    ExecutePartI(agent, iterationState, rankedGoals, processedRules);
                }
            }
        }
    }
}

