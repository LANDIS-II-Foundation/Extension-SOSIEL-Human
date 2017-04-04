﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    using Enums;

    public class InitialStateConfiguration
    {
        public SocialNetworkTypes SocialNetwork { get; set; }

        public bool GenerateGoalProportions { get; set; }

        public AgentStateConfiguration[] AgentsState { get; set; }
    }
}
