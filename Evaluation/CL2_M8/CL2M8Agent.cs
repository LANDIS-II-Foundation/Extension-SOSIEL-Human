﻿using System;
using System.Collections.Generic;
using System.Linq;

using Common.Entities;
using Common.Helpers;
using Common.Exceptions;
using Common.Randoms;
using Common.Configuration;


namespace CL2_M8
{
    public sealed class CL2M8Agent : Agent, ICloneableAgent<CL2M8Agent>
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

        public new CL2M8Agent Clone()
        {
            CL2M8Agent agent = (CL2M8Agent)base.Clone();
      
            agent.PrivateVariables = new Dictionary<string, dynamic>(PrivateVariables);

            return agent;
        }

        protected override Agent CreateInstance()
        {
            return new CL2M8Agent();
        }

       
        public new void GenerateCustomParams()
        {
            this[VariablesUsedInCode.AgentC] = 0;
            this[VariablesUsedInCode.AgentWellbeing] = 0;
        }
    }
}
