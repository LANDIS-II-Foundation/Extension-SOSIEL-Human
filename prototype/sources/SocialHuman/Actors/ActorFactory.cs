using System;
using System.Reflection;

namespace SocialHuman.Actors
{
    using Entities;
    using Models;
    

    static class ActorFactory
    {
        public static Actor Create(ActorParameters parameters, Site[] sites)
        {
            string assemlyName = typeof(Algorithm).Assembly.GetName().Name;
            return (Actor)Activator.CreateInstance(
                assemblyName: typeof(Algorithm).Assembly.FullName,
                typeName: $"{assemlyName}.Actors.{parameters.ClassName}", 
                ignoreCase: false,
                bindingAttr: BindingFlags.Default,
                binder: null,
                args: new object[] { parameters, sites },
                culture: null,
                activationAttributes: null).Unwrap();
            
        }
    }
}
