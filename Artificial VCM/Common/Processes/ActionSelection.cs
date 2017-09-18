using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;
    using Helpers;
    using Enums;
    using Exceptions;


    /// <summary>
    /// Action selection process implementation.
    /// </summary>
    public class ActionSelection : VolatileProcess
    {
        Goal processedGoal;
        GoalState goalState;


        Dictionary<KnowledgeHeuristic, Dictionary<Goal, double>> anticipatedInfluence;

        KnowledgeHeuristic[] matchedHeuristics;


        KnowledgeHeuristic priorPeriodActivatedHeuristic;
        KnowledgeHeuristic heuristicForActivating;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            //Rule[] selected = new Rule[] { };

            //if (goalState.DiffCurrentAndFocal > 0)
            //{
            //    if (matchedHeuristics.Any(r => r == priorPeriodActivatedHeuristic))
            //    {
            //        heuristicForActivating = priorPeriodActivatedHeuristic;
            //        return;
            //    }
            //    else
            //    {
            //        Rule[] temp = matchedHeuristics.Where(r => anticipatedInfluence[r][processedGoal] >= 0).ToArray();

            //        selected = temp.Where(r=> anticipatedInfluence[r][processedGoal] < goalState.DiffCurrentAndFocal).ToArray();

            //        if(selected.Length == 0)
            //        {
            //            selected = temp.Where(r => anticipatedInfluence[r][processedGoal] <= goalState.DiffCurrentAndFocal).ToArray();
            //        }
            //    }
            //}
            //else
            //{
            //    Rule[] temp = matchedHeuristics.Where(r => anticipatedInfluence[r][processedGoal] >= 0).ToArray();

            //    selected = temp.Where(r=> anticipatedInfluence[r][processedGoal] > goalState.DiffCurrentAndFocal).ToArray();

            //    if (selected.Length == 0)
            //    {
            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] >= goalState.DiffCurrentAndFocal).ToArray();
            //    }
            //}

            //if (selected.Length > 0)
            //{
            //    selected = selected.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

            //    heuristicForActivating = selected.RandomizeOne();
            //}





            //We don't do anything. Do nothing heuristic will be selected later.
        }

        protected override void EqualToOrBelowFocalValue()
        {
            //Rule[] selected = new Rule[] { };

            //if (goalState.DiffCurrentAndFocal < 0)
            //{
            //    if (matchedHeuristics.Any(r => r == priorPeriodActivatedHeuristic))
            //    {
            //        heuristicForActivating = priorPeriodActivatedHeuristic;
            //        return;
            //    }
            //    else
            //    {
            //        Rule[] temp = matchedHeuristics.Where(r => anticipatedInfluence[r][processedGoal] <= 0).ToArray();

            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] < Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();

            //        if (selected.Length == 0)
            //        {
            //            selected = temp.Where(r => anticipatedInfluence[r][processedGoal] <= Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
            //        }

            //    }
            //}
            //else
            //{
            //    Rule[] temp = matchedHeuristics.Where(r => anticipatedInfluence[r][processedGoal] <= 0).ToArray();

            //    selected = temp.Where(r => anticipatedInfluence[r][processedGoal] > Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();

            //    if (selected.Length == 0)
            //    {
            //        selected = temp.Where(r => anticipatedInfluence[r][processedGoal] >= Math.Abs(goalState.DiffCurrentAndFocal)).ToArray();
            //    }
            //}

            //if (selected.Length > 0)
            //{
            //    selected = selected.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

            //    heuristicForActivating = selected.RandomizeOne();
            //}

            throw new NotImplementedException("EqualToOrBelowFocalValue is not implemented in ActionSelection");
        }

        protected override void Maximize()
        {
            if (matchedHeuristics.Length > 0)
            {
                KnowledgeHeuristic[] selected = matchedHeuristics.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderByDescending(hg => hg.Key).First().ToArray();

                heuristicForActivating = selected.RandomizeOne();
            }
        }

        protected override void Minimize()
        {
            if (matchedHeuristics.Length > 0)
            {
                KnowledgeHeuristic[] selected = matchedHeuristics.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                heuristicForActivating = selected.RandomizeOne();
            }
        }
        #endregion

        /// <summary>
        /// Shares collective action among same household agents
        /// </summary>
        /// <param name="currentAgent"></param>
        /// <param name="heuristic"></param>
        /// <param name="agentStates"></param>
        void ShareCollectiveAction(IAgent currentAgent, KnowledgeHeuristic heuristic, Dictionary<IAgent, AgentState> agentStates)
        {
            foreach (IAgent neighbour in currentAgent.ConnectedAgents
                .Where(connected => connected[SosielVariables.Network] == currentAgent[SosielVariables.Network]))
            {
                if (neighbour.AssignedKnowledgeHeuristics.Contains(heuristic) == false)
                {
                    neighbour.AssignNewHeuristic(heuristic, currentAgent.AnticipationInfluence[heuristic]);
                }
            }
        }

        /// <summary>
        /// Executes first part of action selection for specific agent and site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedHeuristics"></param>
        /// <param name="site"></param>
        public void ExecutePartI(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration,
            Goal[] rankedGoals, KnowledgeHeuristic[] processedHeuristics, Site site)
        {
            heuristicForActivating = null;

            AgentState agentState = lastIteration.Value[agent];
            AgentState priorPeriod = lastIteration.Previous?.Value[agent];

            //adds new heuristic history for specific site if it doesn't exist
            if (agentState.HeuristicHistories.ContainsKey(site) == false)
                agentState.HeuristicHistories.Add(site, new KnowledgeHeuristicsHistory());

            KnowledgeHeuristicsHistory history = agentState.HeuristicHistories[site];

            processedGoal = rankedGoals.First(g => processedHeuristics.First().Layer.Set.AssociatedWith.Contains(g));
            goalState = agentState.GoalsState[processedGoal];

            matchedHeuristics = processedHeuristics.Except(history.BlockedHeuristics).Where(h => h.IsMatch(agent)).ToArray();

            if (matchedHeuristics.Length > 1)
            {
                priorPeriodActivatedHeuristic = priorPeriod.HeuristicHistories[site].Activated.FirstOrDefault(r => r.Layer == processedHeuristics.First().Layer);
                
                //set anticipated influence before execute specific logic
                anticipatedInfluence = agent.AnticipationInfluence;

                SpecificLogic(processedGoal.Tendency);

                //if none are identified, then choose the do-nothing heuristic.
                if (heuristicForActivating == null)
                {
                    try
                    {
                        heuristicForActivating = processedHeuristics.Single(h => h.IsAction == false);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new SosielAlgorithmException(string.Format("Heuristic for activating hasn't been found by {0}", agent.Id));
                    }
                }
            }
            else
                heuristicForActivating = matchedHeuristics[0];

            if (processedHeuristics.First().Layer.Set.Layers.Count > 1)
                heuristicForActivating.Apply(agent);


            if (heuristicForActivating.IsCollectiveAction)
            {
                ShareCollectiveAction(agent, heuristicForActivating, lastIteration.Value);
            }


            history.Activated.Add(heuristicForActivating);
            history.Matched.AddRange(matchedHeuristics);
        }

        /// <summary>
        /// Executes second part of action selection for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedHeuristics"></param>
        /// <param name="site"></param>
        public void ExecutePartII(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration,
            Goal[] rankedGoals, KnowledgeHeuristic[] processedHeuristics, Site site)
        {
            AgentState agentState = lastIteration.Value[agent];

            KnowledgeHeuristicsHistory history = agentState.HeuristicHistories[site];

            KnowledgeHeuristicsLayer layer = processedHeuristics.First().Layer;


            KnowledgeHeuristic selectedHeuristic = history.Activated.Single(r => r.Layer == layer);

            if (selectedHeuristic.IsCollectiveAction)
            {
                //counting agents which selected this heuristic
                int numberOfInvolvedAgents = agent.ConnectedAgents.Where(connected=> agent[SosielVariables.Network] == connected[SosielVariables.Network])
                    .Count(a=> lastIteration.Value[a].HeuristicHistories[site].Activated.Any(heuristic=> heuristic == selectedHeuristic));

                int requiredParticipants = selectedHeuristic.RequiredParticipants - 1;

                //add heuristic to blocked heuristics
                if (numberOfInvolvedAgents < requiredParticipants)
                {
                    history.BlockedHeuristics.Add(selectedHeuristic);

                    history.Activated.Remove(selectedHeuristic);

                    ExecutePartI(agent, lastIteration, rankedGoals, processedHeuristics, site);

                    ExecutePartII(agent, lastIteration, rankedGoals, processedHeuristics, site);
                }
            }
        }
    }
}

