using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace SCIL.Processor
{
    public static class DependencyResolver
    {
        public static IServiceCollection RegistrerAllTypes<T>(this IServiceCollection services)
        {
            var type = typeof(T);

            // Get the assembly and all defined types which matches this type (implemented interfaces or baseType)
            var foundTypes =
                from definedType in type.Assembly.DefinedTypes
                where definedType.IsPublic
                where !definedType.IsAbstract
                where type.IsAssignableFrom(definedType)
                select definedType;

            // Registrer services
            foreach (var foundType in foundTypes)
            {
                services.AddSingleton(foundType, foundType);
                services.AddSingleton(type, serviceProvider => serviceProvider.GetRequiredService(foundType));
            }

            return services;
        }

        public static T Resolve<T>(this IServiceProvider serviceProvider)
        {
            return (T)Resolve(serviceProvider, typeof(T));
        }

        public static object Resolve(this IServiceProvider serviceProvider, Type type)
        {
            var serviceType = serviceProvider.GetService(type);
            if (serviceType != null)
                return serviceType;

            // Check if constructor with 0 arguments was found
            if (type.GetConstructors().Any(e => e.IsPublic && e.GetParameters().Length == 0))
            {
                return Activator.CreateInstance(type);
            }

            // Get all constructors
            var constructors = type.GetConstructors().Where(e => e.IsPublic);
            foreach (var constructor in constructors)
            {
                var arguments = new List<object>();
                var foundAll = true;
                foreach (var argument in constructor.GetParameters())
                {
                    var resolvedArgument = Resolve(serviceProvider, argument.ParameterType);
                    if (resolvedArgument == null)
                    {
                        foundAll = false;
                        break;
                    }
                    arguments.Add(resolvedArgument);
                }

                if (foundAll)
                {
                    return Activator.CreateInstance(type, arguments.ToArray());
                }
            }

            throw new NotSupportedException("Cound not resolve constructor for type " + type);
        }
    }
}
