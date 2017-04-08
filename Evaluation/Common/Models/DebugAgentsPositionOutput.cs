using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{


    [DelimitedRecord(";")]
    public class DebugAgentsPositionOutput
    {
        public string Positions;
    }
}
