using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace SocialHuman.Parsers.Models
{
    class Actor
    {
        #region Private fields

        #endregion

        #region Public fields
        [JsonProperty("class", Required = Required.Always)]
        public string Class { get; private set; }

        [JsonProperty("variables", Required = Required.Always)]
        public Dictionary<string, dynamic> Variables { get; private set; }

        [JsonProperty("goals_state", Required = Required.Always)]
        public GoalState[] AssignedGoals { get; private set; }

        [JsonProperty("assigned_heuristics", Required = Required.Always)]
        public string[] AssagnedHeuristics { get; protected set; }

        [JsonProperty("anticipated_influence", Required = Required.Always)]
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluences { get; private set; }
        #endregion

        #region Constructors
        
        #endregion

        #region Public methods
        
        #endregion

    }
}
