using System.Collections.Generic;

namespace Common.Entities
{
    using Environment;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }
        int Id { get; set; }
        List<Rule> Rules { get; set; }
        //Dictionary<string, dynamic> Variables { get; set; }

        void GenerateCustomParams();
    }
}