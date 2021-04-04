using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation
{
    /// <summary>
    /// MediatR pipeline that handles canceled requests.
    /// </summary>
    /// <typeparam name="TRequest">The type of cancelable request being handled.</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    public class CancelableRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICancelableRequest<TResponse>
    {
        private readonly IResponseFinalizer<TRequest, TResponse> finalizer;

        /// <summary>
        /// Creates a new <see cref="CancelableRequestBehavior{TRequest, TResponse}"/> to handle canceled requests.
        /// </summary>
        /// <param name="finalizer">
        /// The response finalizer used to do finalization work on cancellation.
        /// <para><i>If no finalizer has been registered for the request, than the default passthrough finalizer will be used.</i></para>
        /// </param>
        public CancelableRequestBehavior(IResponseFinalizer<TRequest, TResponse> finalizer)
        {
            this.finalizer = finalizer;
        }

        /// <summary>
        /// Pipeline handler. 
        /// </summary>
        /// <param name="request">Incoming cancelable request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="next">Awaitable delegate for the next action in the pipeline.</param>
        /// <returns>Awaitable task returning the TResponse.</returns>
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return await finalizer.Finalize(request);
            }
        }
    }
}