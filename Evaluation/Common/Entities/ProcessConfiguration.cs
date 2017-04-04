using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class ProcessConfiguration
    {
        public bool AgentRandomizationEnabled { get; set; }

        public bool AnticipatoryLearningEnabled { get; set; }

        public bool CounterfactualThinkingEnabled { get; set; }

        public bool InnovationEnabled { get; set; }

        public bool SocialLearningEnabled { get; set; }

        public bool RuleSelectionEnabled { get; set; }

        public bool RuleSelectionPart2Enabled { get; set; }

        public bool ActionTakingEnabled { get; set; }

        public bool ReproductionEnabled { get; set; }

        public bool AgentsDeactivationEnabled { get; set; }
    }
}
