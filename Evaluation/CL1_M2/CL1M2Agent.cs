using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;
using Common.Helpers;

namespace CL1_M2
{
    public sealed class CL1M2Agent : Agent, ICloneableAgent<CL1M2Agent>
    {
        private Dictionary<string, dynamic> PrivateVariables { get; set; } = new Dictionary<string, dynamic>();

        public override dynamic this[string key]
        {
            get
            {
                if (PrivateVariables.ContainsKey(key))
                    return PrivateVariables[key];
                else
                {
                    return base[key];
                }
            }

            set
            {
                if (Variables.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    if (PrivateVariables.ContainsKey(key))
                        PreSetValue(key, PrivateVariables[key]);

                    PrivateVariables[key] = value;

                    PostSetValue(key, value);
                }

            }
        }

        public override bool ContainsVariable(string key)
        {
            return PrivateVariables.ContainsKey(key) || base.ContainsVariable(key);
        }

        public new CL1M2Agent Clone()
        {
            CL1M2Agent agent = (CL1M2Agent)base.Clone();

            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL1M2Agent();
        }

        public new void GenerateCustomParams()
        {
            AgentSubtype agentSubtype = (AgentSubtype)LinearUniformRandom.GetInstance.Next(1, 3);

            this[VariablesUsedInCode.AgentSubtype] = agentSubtype;

            if (agentSubtype == AgentSubtype.Co)
            {
                this[VariablesUsedInCode.AgentC] = this[VariablesUsedInCode.Endowment];
            }
            else
            {
                this[VariablesUsedInCode.AgentC] = 0;
            }
        }
    }
}
