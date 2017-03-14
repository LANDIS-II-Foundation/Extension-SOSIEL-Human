using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    [DelimitedRecord(";")]
    public class SimpleLineOutput
    {
        public string Value { get; set; }

        public SimpleLineOutput(object obj)
        {
            Value = obj.ToString();
        }
    }
}
