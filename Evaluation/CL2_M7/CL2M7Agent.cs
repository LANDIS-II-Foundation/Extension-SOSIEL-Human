using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Helpers;



namespace CL2_M7
{
    public sealed class CL2M7Agent : Agent, ICloneableAgent<CL2M7Agent>
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

        public new CL2M7Agent Clone()
        {
            CL2M7Agent agent = (CL2M7Agent)base.Clone();

            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL2M7Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentC] = 0;
            this[VariablesUsedInCode.AgentWellbeing] = 0;
            this[$"{VariablesUsedInCode.PreviousPrefix}_{VariablesUsedInCode.AgentC}"] = 0;
        }
    }
}
