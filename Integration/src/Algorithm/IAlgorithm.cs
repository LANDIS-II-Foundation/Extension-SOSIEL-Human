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

        void Initialize();

        void RunIteration();
    }
}
