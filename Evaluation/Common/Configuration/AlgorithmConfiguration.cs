using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Common.Configuration
{
    using Enums;

    public class AlgorithmConfiguration
    {
        public Model Model { get; set; }

        public int NumberOfIterations { get; set; }

        public double VacantProportion { get; set; }
    }
}
