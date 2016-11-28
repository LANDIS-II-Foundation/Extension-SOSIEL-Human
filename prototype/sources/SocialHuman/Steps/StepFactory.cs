using System;
using System.Reflection;

namespace SocialHuman.Steps
{
    using Models;
    using Steps.Abstract;
    

    static class StepFactory
    {
        public static object Create(string type, string className)
        {
            string assemlyName = typeof(Algorithm).Assembly.GetName().Name;

            return Activator.CreateInstance(
                assemblyName: typeof(Algorithm).Assembly.FullName,
                typeName: $"{assemlyName}.Steps.{type}.{className}",
                ignoreCase: false,
                bindingAttr: BindingFlags.Default,
                binder: null,
                args: new object[] { },
                culture: null,
                activationAttributes: null).Unwrap();
        }
    }
}
