using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Randoms;
using Common.Helpers;

namespace CL1_M1
{
    public sealed class CL1M1Agent : Agent, ICloneableAgent<CL1M1Agent>
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

        public new CL1M1Agent Clone()
        {
            CL1M1Agent agent = (CL1M1Agent)base.Clone();

            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL1M1Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentSubtype] = (AgentSubtype)LinearUniformRandom.GetInstance.Next(1, 3);
        }
    }
}
