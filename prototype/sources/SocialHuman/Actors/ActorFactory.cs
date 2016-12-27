using System;
using System.Reflection;

namespace SocialHuman.Actors
{
    using Models;
    using Models.Input;
    

    static class ActorFactory
    {
        public static Actor Create(string actorType, ActorInput input, Site[] sites)
        {
            string assemblyName = typeof(Algorithm).Assembly.GetName().Name;
            return (Actor)Activator.CreateInstance(
                assemblyName: typeof(Algorithm).Assembly.FullName,
                typeName: $"{assemblyName}.Actors.{actorType}", 
                ignoreCase: false,
                bindingAttr: BindingFlags.Default,
                binder: null,
                args: new object[] { input, sites },
                culture: null,
                activationAttributes: null).Unwrap();
            
        }
    }
}
