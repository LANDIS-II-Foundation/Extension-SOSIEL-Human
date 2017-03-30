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

        IEnumerable<Goal> RankGoal(AgentState state)
        {
            int numberOfGoal = state.GoalsState.Count;

            List<Goal> vector = new List<Goal>(100);

            state.GoalsState.ForEach(kvp =>
            {
                int numberOfInsertions = Convert.ToInt32(Math.Round(kvp.Value.Proportion * 100));

                for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
            });

            for (int i = 0; i < numberOfGoal; i++)
            {
                Goal nextGoal = vector.RandomizeOne();

                vector.RemoveAll(o => o == nextGoal);

                yield return nextGoal;
            }
        } 



        //void ShareCollectiveAction(ICollection<Actor> sameTypeActors, Actor currentActor, Heuristic heuristic)
        //{
        //    string socialNetwork = ((string[])currentActor[VariableNames.SocialNetworks])[0];

        //    foreach (Actor actor in sameTypeActors.Where(a => ((string[])a[VariableNames.SocialNetworks]).Any(sn => sn == socialNetwork) && a != currentActor))
        //    {
        //        actor.AssignHeuristic(currentActor, heuristic);
        //    }
        //}

        public void ExecutePartI(IConfigurableAgent agent, ICollection<IConfigurableAgent> sameTypeActors,
            LinkedListNode<Dictionary<IConfigurableAgent, AgentState>> periodState, Goal[] rankedGoals,
            Rule[] processedRules)
        {
            ruleForActivating = null;

            agentState = periodState.Value[agent];
            AgentState priorPeriod = periodState.Previous.Value[agent];

            if(rankedGoals == null)
            {
                rankedGoals = RankGoal(agentState).ToArray();
            }



            processedGoal = rankedGoals.First(g=> processedRules.First().Layer.Set.AssociatedWith.Contains(g));
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


            //todo     wrong implementation. 
            //ruleForActivating.Apply(agent);

            //if (heuristicForActivating.IsCollectiveAction)
            //{
            //    ShareCollectiveAction(sameTypeActors, actor, heuristicForActivating);
            //}


            //SiteState siteState = currentPeriod.GetStateForSite(actor, site);

            agentState.Activated.Add(ruleForActivating);
            agentState.Matched.AddRange(matchedRules);
        }

        //public void ExecutePartII(Actor actor, ICollection<Actor> sameTypeActors, LinkedListNode<Period> periodModel, GoalState goalState, IEnumerable<Heuristic> layer, Site site = null)
        //{
        //    Period currentPeriod = periodModel.Value;

        //    HeuristicLayer heuristicLayer = layer.First().Layer;

        //    Heuristic selectedHeuristic = currentPeriod.GetStateForSite(actor, site).Activated.Single(h => h.Layer == heuristicLayer);

        //    if (selectedHeuristic.IsCollectiveAction)
        //    {
        //        int actorsCount = sameTypeActors.Count(a => currentPeriod.GetStateForSite(a, site).Activated.Single(h => h.Layer == heuristicLayer) == selectedHeuristic);

        //        if (actorsCount + 1 < selectedHeuristic.RequiredParticipants)
        //        {
        //            actor.BlockedHeuristics.Add(selectedHeuristic);
        //            SiteState siteState = currentPeriod.GetStateForSite(actor, site);

        //            siteState.Activated.Clear();
        //            siteState.Matched.Clear();

        //            ExecutePartI(actor, sameTypeActors, periodModel, goalState, layer, site);
        //        }
        //    }
        //}
    }
}

