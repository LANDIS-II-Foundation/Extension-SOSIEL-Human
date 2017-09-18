using System.Collections.Generic;

namespace Common.Entities
{
    using Exceptions;
    using Helpers;


    public sealed class AgentState
    {
        public Dictionary<Goal, GoalState> GoalsState { get; private set; }

        public Dictionary<Site, KnowledgeHeuristicsHistory> HeuristicHistories { get; private set; }

        public Dictionary<Site, List<TakenAction>> TakenActions { get; private set; }


        public bool IsSiteOriented { get; private set; }


        private AgentState()
        {
            GoalsState = new Dictionary<Goal, GoalState>();

            HeuristicHistories = new Dictionary<Site, KnowledgeHeuristicsHistory>();

            TakenActions = new Dictionary<Site, List<TakenAction>>();
        }


        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented)
        {
            return new AgentState { IsSiteOriented = isSiteOriented };
        }



        /// <summary>
        /// Creates agent state with one heuristic history. For not site oriented agents only.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, KnowledgeHeuristicsHistory history)
        {
            if (isSiteOriented)
                throw new SosielAlgorithmException("Wrong AgentState.Create method usage");

            AgentState state = Create(isSiteOriented);

            state.HeuristicHistories.Add(Site.DefaultSite, history); 

            return state;
        }

        /// <summary>
        /// Creates agent state with heuristic histories related to sites.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, Dictionary<Site, KnowledgeHeuristicsHistory> history)
        {
            AgentState state = Create(isSiteOriented);

            state.HeuristicHistories = new Dictionary<Site, KnowledgeHeuristicsHistory>(history);

            return state;
        }


        /// <summary>
        /// Adds heuristic history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        public void AddHeuristicHistory(KnowledgeHeuristicsHistory history)
        {
            if (IsSiteOriented)
                throw new SosielAlgorithmException("Couldn't add heuristic history without site. It isn't possible for site oriented agents.");

            HeuristicHistories.Add(Site.DefaultSite, history);
        }


        /// <summary>
        /// Adds heuristic history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddHeuristicHistory(KnowledgeHeuristicsHistory history, Site site)
        {
            HeuristicHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState CreateForNextIteration()
        {
            AgentState agentState = Create(IsSiteOriented);

            GoalsState.ForEach(kvp =>
            {
                agentState.GoalsState.Add(kvp.Key, kvp.Value.CreateForNextIteration());
            });

            HeuristicHistories.Keys.ForEach(site =>
            {
                agentState.HeuristicHistories.Add(site, new KnowledgeHeuristicsHistory());
            });

            return agentState;
        }
    }
}
