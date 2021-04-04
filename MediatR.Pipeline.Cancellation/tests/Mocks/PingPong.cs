using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public class Ping : CancelableRequest
    {
       
    }

    public class Pong : IRequestHandler<Ping, Unit>
    {
        public async Task<Unit> Handle(Ping request, CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(50);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    public class PingFinalizer : IResponseFinalizer<Ping>
    {
        public async Task<Unit> Finalize(Ping request)
        {
            await Task.Delay(10); // Do some finalization work (maybe call a web service).
            return Unit.Value;
        }
    }
}
