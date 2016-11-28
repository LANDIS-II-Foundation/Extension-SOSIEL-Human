namespace Demo.Parsers
{
    using Models.Input;

    interface IParser
    {
        GlobalInput ParseGlogalConfiguration();

        ActorInput[] ParseActors();

    }
}
