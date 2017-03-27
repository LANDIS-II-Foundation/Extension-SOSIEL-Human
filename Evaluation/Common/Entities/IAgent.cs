using System.Collections.Generic;

namespace Common.Entities
{
    using Environments;

    public interface IAgent
    {
        dynamic this[string key] { get; set; }
        int Id { get; set; }
        List<Rule> Rules { get; set; }

        List<Goal> Goals { get; set; }

        void GenerateCustomParams();
    }
}