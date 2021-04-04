using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public class Hello : ICancelableRequest<string>
    {
        public string YourName { get; set; }
        public string Response { get; set; }
    }

    public class World : IRequestHandler<Hello, string>
    {
        public async Task<string> Handle(Hello request, CancellationToken cancellationToken)
        {
            request.Response = $"Hello {request.YourName} - Im in the handler now";

            await Task.Delay(100);
            cancellationToken.ThrowIfCancellationRequested();

            request.Response += " - Im leaving the handler now";
            return request.Response;
        }
    }

    public class HelloFinalizers : IResponseFinalizer<Hello, string>
    {
        public async Task<string> Finalize(Hello request)
        {
            request.Response += " - Im in the finalizer now";
            await Task.Delay(100);
            return request.Response + " - Im leaving the finalizer now";
        }
    }
}
