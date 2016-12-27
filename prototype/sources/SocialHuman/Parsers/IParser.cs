using System;
using System.Collections.Generic;

namespace SocialHuman.Parsers
{
    using Models;

    interface IParser
    {
        GlobalInput ParseGlogalConfiguration();
        Dictionary<string, ActorPrototype> ParseActorPrototypes();
        Dictionary<string, InitialState> ParseInitialState();
    }
}
