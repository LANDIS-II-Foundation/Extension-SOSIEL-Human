using System.Collections.Generic;

namespace Common.Entities
{
    using Configuration;
    using Environments;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }

        string Id { get; }

        List<IAgent> ConnectedAgents { get; }

        Dictionary<KnowledgeHeuristic, Dictionary<Goal, double>> AnticipationInfluence { get; }

        List<Goal> AssignedGoals { get; }

        List<KnowledgeHeuristic> AssignedKnowledgeHeuristics { get; }

        Dictionary<KnowledgeHeuristic, int> KnowledgeHeuristicActivationFreshness { get; }

        AgentPrototype Prototype { get; }

        Dictionary<Goal, GoalState> InitialGoalStates { get; }

        /// <summary>
        /// Assigns new heuristic to mental model (heuristic list) of current agent. If empty rooms ended, old heuristics will be removed.
        /// </summary>
        /// <param name="newHeuristic"></param>
        void AssignNewHeuristic(KnowledgeHeuristic newHeuristic);

        /// <summary>
        /// Assigns new heuristic with defined anticipated influence to mental model (heuristic list) of current agent. If empty rooms ended, old heuristics will be removed. 
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="anticipatedInfluence"></param>
        void AssignNewHeuristic(KnowledgeHeuristic newHeuristic, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Adds heuristic to prototype heuristics and then assign one to the heuristic list of current agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="layer"></param>
        void AddHeuristic(KnowledgeHeuristic newHeuristic, KnowledgeHeuristicsLayer layer);


        /// <summary>
        /// Adds heuristic to prototype heuristics and then assign one to the heuristic list of current agent. 
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newHeuristic"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        void AddHeuristic(KnowledgeHeuristic newHeuristic, KnowledgeHeuristicsLayer layer, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetToCommon(string key, dynamic value);

        /// <summary>
        /// Check on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsVariable(string key);
    }
}