using Landis.SpatialModeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Landis.Extension.SOSIELHuman.Algorithm
{
    public interface IAlgorithm
    {
        string Name { get; }

        /// <summary>
        /// Initialization of algorithm
        /// </summary>
        void Initialize();

        /// <summary>
        /// Runs as many iterations as passed to the constructor
        /// </summary>
        void RunIteration();
    }
}
