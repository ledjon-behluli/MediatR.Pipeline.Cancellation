using Microsoft.Extensions.DependencyInjection;
using MediatR.Pipeline.Cancellation.Tests.Mocks;
using System.Reflection;

namespace MediatR.Pipeline.Cancellation.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBlobStorageProvider, MockedBlobStorage>();
            services.AddSingleton<IDatabaseProvider, MockedDatabase>();

            Assembly assembly = typeof(Startup).Assembly;

            services.AddMediatR(assembly);
            services.AddCancellationPipeline(assembly);
        }
    }
}
