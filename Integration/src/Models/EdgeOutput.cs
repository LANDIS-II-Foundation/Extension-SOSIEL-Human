using System;
using System.Collections.Generic;
using System.Linq;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    [DelimitedRecord(";")]
    public class EdgeOutput :IHeader
    {
        public string HeaderLine
        {
            get
            {
                return "ID1;ID2";
            }
        }

        public int AgentId { get; set; }

        public int AdjacentAgentId { get; set; }

        
    }
}
