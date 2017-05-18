using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{


    [DelimitedRecord(";")]
    public class DebugAgentsPositionOutput
    {
        public string Positions;
    }
}
