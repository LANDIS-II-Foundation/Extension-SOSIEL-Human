using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;
    using Randoms;
    using Helpers;


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

            if (goalState.DiffCurrentAndMin > 0)
            {
                if (matchedRules.Any(r => r == priorPeriodActivatedRule))
                {
                    ruleForActivating = priorPeriodActivatedRule;
                    return;
                }
                else
                {
                    selected = matchedRules.Where(r => ai[r][processedGoal] >= 0 &&
                        ai[r][processedGoal] < goalState.DiffCurrentAndMin).ToArray();
                }
            }
            else
            {
                selected = matchedRules.Where(r => ai[r][processedGoal] >= 0 &&
                    ai[r][processedGoal] > goalState.DiffCurrentAndMin).ToArray();
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

            if (goalState.DiffCurrentAndMin < 0)
            {
                if (matchedRules.Any(r => r == priorPeriodActivatedRule))
                {
                    ruleForActivating = priorPeriodActivatedRule;
                    return;
                }
                else
                {
                    selected = matchedRules.Where(r => ai[r][processedGoal] <= 0 &&
                        Math.Abs(ai[r][processedGoal]) < Math.Abs(goalState.DiffCurrentAndMin)).ToArray();
                }
            }
            else
            {
                selected = matchedRules.Where(r => ai[r][processedGoal] < 0 &&
                    Math.Abs(ai[r][processedGoal]) > Math.Abs(goalState.DiffCurrentAndMin)).ToArray();
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

        //IEnumerable<Goal> RankGoal(AgentState state)
        //{
        //    int numberOfGoal = state.GoalsState.Count;

        //    List<Goal> vector = new List<Goal>(100);

        //    state.GoalsState.ForEach(kvp =>
        //    {
        //        int numberOfInsertions = Convert.ToInt32(Math.Round(kvp.Value.Proportion * 100));

        //        for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
        //    });

        //    for (int i = 0; i < numberOfGoal; i++)
        //    {
        //        Goal nextGoal = vector.RandomizeOne();

        //        vector.RemoveAll(o => o == nextGoal);

        //        yield return nextGoal;
        //    }
        //}



        void ShareCollectiveAction(IAgent currentAgent, Rule rule, Dictionary<IAgent, AgentState> agentStates)
        {
            foreach (IAgent agent in currentAgent.ConnectedAgents)
            {
                if (agent.AssignedRules.Contains(rule) == false)
                {
                    agent.AssignedRules.Add(rule);

                    agentStates[agent].AnticipationInfluence.Add(rule, new Dictionary<Goal, double>(agentStates[currentAgent].AnticipationInfluence[rule]));
                }
            }
        }

        public void ExecutePartI(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> iterationState,
            Goal[] rankedGoals, Rule[] processedRules)
        {
            ruleForActivating = null;

            agentState = iterationState.Value[agent];
            AgentState priorPeriod = iterationState.Previous?.Value[agent];

            //if (rankedGoals == null)
            //{
            //    rankedGoals = RankGoal(agentState).ToArray();
            //}



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

            //activatedHeuristic.FreshnessStatus = 0;


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
            Goal[] rankedGoals, Rule[] processedRules, int numberOfAgents)
        {
            AgentState agentState = iterationState.Value[agent];

            RuleLayer layer = processedRules.First().Layer;

            Rule selectedRule = agentState.Activated.Single(r => r.Layer == layer);

            if (selectedRule.IsCollectiveAction)
            {
                int numberOfInvolvedAgents = agent.ConnectedAgents.Count(a => iterationState.Value[a].Activated.Single(r => r.Layer == layer) == selectedRule);

                int requiredParticipants = selectedRule.RequiredParticipants;


                // Value 0 means all agents who participate in algorithm
                if (requiredParticipants == 0)
                {
                    requiredParticipants = numberOfAgents;
                }



                if (numberOfInvolvedAgents < requiredParticipants - 1)
                {
                    agentState.BlockedRules.Add(selectedRule);

                    agentState.Activated.Remove(selectedRule);
                    
                    ExecutePartI(agent, iterationState, rankedGoals, processedRules);
                }
            }
        }
    }
}

