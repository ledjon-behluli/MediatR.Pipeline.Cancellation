using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Pipeline.Cancellation
{
    /// <summary>
    /// Extension class for registering the pipeline and all finalizers.
    /// </summary>
    public static class ServiceRegistrations
    {
        /// <summary>
        /// Registers <see cref="CancelableRequestBehavior{TRequest, TResponse}"/> and all <see cref="IResponseFinalizer{TRequest, TResponse}"/> for the given assemblies.
        /// </summary>
        public static IServiceCollection AddCancellationPipeline(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CancelableRequestBehavior<,>));
            services.AddResponseFinalizers(assemblies);

            return services;
        }

        private static void AddResponseFinalizers(this IServiceCollection services, params Assembly[] assemblies)
        {
            var type = typeof(IResponseFinalizer<,>);

            services.AddTransient(typeof(IResponseFinalizer<,>), typeof(PassThroughFinalizer<,>));

            foreach (Assembly assembly in assemblies)
                GetTypesAssignableTo(assembly, type)
                    .ForEach((implementationType) =>
                    {
                        foreach (var serviceType in implementationType.ImplementedInterfaces)
                        {
                            if (!serviceType.IsGenericType)
                                continue;

                            if (serviceType.GetGenericTypeDefinition() != type)
                                continue;

                            services.AddTransient(serviceType, implementationType);
                        }
                    });
        }

        private static List<TypeInfo> GetTypesAssignableTo(Assembly assembly, Type compareType)
        {
            return assembly.DefinedTypes.Where(x => x.IsClass
                                && !x.IsAbstract
                                && x != compareType
                                && x.GetInterfaces()
                                        .Any(i => i.IsGenericType
                                                && i.GetGenericTypeDefinition() == compareType))?.ToList();
        }
    }
}