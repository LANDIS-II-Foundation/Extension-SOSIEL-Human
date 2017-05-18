using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using FileHelpers;

namespace Landis.Extension.Sosiel.Models
{
    class ToStringConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            throw new NotImplementedException();
        }

        public override string FieldToString(object fieldValue)
        {
            if(fieldValue is double)
            {
                return string.Format("\"{0:0.000}\"", fieldValue);
            }

            return fieldValue.ToString();
        }
    }
}
