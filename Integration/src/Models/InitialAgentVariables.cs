using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    public class VariableItem
    {
        public string VariableName { get; set; }

        public dynamic Value { get; set; }


        public override string ToString()
        {
            return $"{(Value.GetType() == typeof(double) ? Value.ToString("0.000") : Value.ToString())}";
        }
    }

    [DelimitedRecord(";")]
    public class InitialAgentVariables : IHeader
    {
        public string HeaderLine
        {
            get
            {
                return $"\"ID\";{ (VariableItems != null ? string.Join(";", VariableItems.Select(vi => vi.VariableName)) : "") }";
            }
        }

        public int ID;

        [FieldConverter(typeof(ToStringConverter))]
        public VariableItem[] VariableItems;
    }
}
