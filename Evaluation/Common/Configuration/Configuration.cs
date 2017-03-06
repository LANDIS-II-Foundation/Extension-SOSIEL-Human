using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Configuration
{
    public class Configuration<T> where T: class
    {
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }

        public T AgentConfiguration { get; set; }

    }
}
