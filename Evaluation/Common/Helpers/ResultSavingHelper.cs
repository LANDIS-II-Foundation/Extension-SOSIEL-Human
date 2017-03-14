using System;
using System.Collections.Generic;
using System.Linq;

using FileHelpers;

namespace Common.Helpers
{
    public static class ResultSavingHelper
    {
        public static void Save<T>(IEnumerable<T> data, string fileName) where T:class
        {
            DelimitedFileEngine<T> engine = new DelimitedFileEngine<T>();

            engine.WriteFile(fileName, data);
        }

    }
}
