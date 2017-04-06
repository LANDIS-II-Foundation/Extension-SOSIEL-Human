using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Exceptions;
using Common.Randoms;

namespace CL1_M4
{
    public sealed class CL1M4Agent : Agent, ICloneableAgent<CL1M4Agent>
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
                    PrivateVariables[key] = value;
                }

            }
        }

        public new CL1M4Agent Clone()
        {
            return (CL1M4Agent)base.Clone();
        }

        protected override Agent CreateInstance()
        {
            return new CL1M4Agent();
        }

        public new void GenerateCustomParams()
        {
            int randomType = LinearUniformRandom.GetInstance.Next(1, 4);

            AgentSubtype subtype = (AgentSubtype)randomType;

            this[VariablesUsedInCode.AgentSubtype] = subtype;

            this[VariablesUsedInCode.AgentC] = 0;
            this[VariablesUsedInCode.AgentP] = 0;


            if (subtype == AgentSubtype.Co || subtype == AgentSubtype.Enf)
            {
                this[VariablesUsedInCode.AgentC] = this[VariablesUsedInCode.Endowment];

                if (subtype == AgentSubtype.Enf)
                {
                    this[VariablesUsedInCode.AgentP] = this[VariablesUsedInCode.Punishment];
                }
            }
        }
    }
}
