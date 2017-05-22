using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Output
{
    public class FEValuesOutput
    {
        public int Iteration { get; set; }

        public string Site { get; set; }

        public double Biomass { get; set; }

        public double ReductionPercentage { get; set; }

        public double BiomassReduction { get; set; }

        public double Profit { get; set; }
    }
}
