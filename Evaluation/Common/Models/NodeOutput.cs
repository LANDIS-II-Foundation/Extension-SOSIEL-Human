using System;
using System.Collections.Generic;
using System.Linq;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class NodeOutput: IHeader
    {
        public string HeaderLine
        {
            get
            {
                return "\"ID\";Type";
            }
        }

        public int AgentId { get; set; }

        public string Type { get; set; }
    }
}
