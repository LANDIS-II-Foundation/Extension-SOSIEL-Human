using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    public class AgentStateConfiguration
    {
        public string PrototypeOfAgent { get; set; }

        public int NumberOfAgents { get; set; }

        public Dictionary<string, dynamic> PrivateVariables { get; set; }


        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        public string[] AssignedRules { get; set; }



        public string[] AssignedGoals { get; set; }

        public string[] ActivatedRulesOnFirstIteration { get; set; }

        public Dictionary<string, GoalStateConfiguration> GoalState { get; set; }

    }
}
