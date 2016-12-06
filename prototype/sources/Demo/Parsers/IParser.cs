using System;
using System.Collections.Generic;

namespace Demo.Parsers
{
    using Models.Input;

    interface IParser
    {
        GlobalInput ParseGlogalConfiguration();
        ActorInput[] ParseActors();
        Dictionary<string, PeriodInitialStateInput> ParseInitialState();
    }
}
