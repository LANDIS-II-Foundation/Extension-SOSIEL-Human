using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Common.Models
{
    class ToStringConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            throw new NotImplementedException();
        }

        public override string FieldToString(object fieldValue)
        {
            return fieldValue.ToString();
        }
    }
}
