using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    public class KnowledgeHeuristicsHistory
    {
        public List<KnowledgeHeuristic> Matched { get; private set; }
        public List<KnowledgeHeuristic> Activated { get; private set; }

        public List<KnowledgeHeuristic> BlockedHeuristics { get; private set; }

        public KnowledgeHeuristicsHistory()
        {
            Matched = new List<KnowledgeHeuristic>();
            Activated = new List<KnowledgeHeuristic>();

            BlockedHeuristics = new List<KnowledgeHeuristic>();
        }

        public KnowledgeHeuristicsHistory(IEnumerable<KnowledgeHeuristic> matched, IEnumerable<KnowledgeHeuristic> activated) : base()
        {
            Matched.AddRange(matched);
            Activated.AddRange(activated);
        }
    }
}
