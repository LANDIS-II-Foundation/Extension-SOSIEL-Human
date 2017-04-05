﻿using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Helpers;
using Common.Exceptions;
using Common.Randoms;
using Common.Configuration;
using Common.Environments;

namespace CL4_M11
{
    public sealed class CL4M11Agent : Agent, ICloneableAgent<CL4M11Agent>
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

        public new CL4M11Agent Clone()
        {
            CL4M11Agent agent = (CL4M11Agent)base.Clone();

            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL4M11Agent();
        }

        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentSubtype] = AgentSubtype.NonCo;
            this[VariablesUsedInCode.AgentC] = 0;
            this[VariablesUsedInCode.AgentWellbeing] = 0;
        }
    }
}
