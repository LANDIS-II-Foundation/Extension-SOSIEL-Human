using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;
using Common.Helpers;

namespace CL1_M5
{
    public sealed class CL1M5Agent : Agent, ICloneableAgent<CL1M5Agent>
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

        public new CL1M5Agent Clone()
        {
            CL1M5Agent agent = (CL1M5Agent)base.Clone();

            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL1M5Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentC] = LinearUniformRandom.GetInstance.Next(this[VariablesUsedInCode.Endowment] + 1);
            this[VariablesUsedInCode.AgentP] = LinearUniformRandom.GetInstance.Next(this[VariablesUsedInCode.Endowment] + 1);

            if (this[VariablesUsedInCode.AgentC] + this[VariablesUsedInCode.AgentP] == 0)
                this[VariablesUsedInCode.AgentSubtype] = AgentSubtype.NonCo;
            else
                this[VariablesUsedInCode.AgentSubtype] = AgentSubtype.Unknown;
        }
    }
}
