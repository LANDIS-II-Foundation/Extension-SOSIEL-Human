using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public interface IAnticipatedInfluenceState
    {
        Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }
    }
}
