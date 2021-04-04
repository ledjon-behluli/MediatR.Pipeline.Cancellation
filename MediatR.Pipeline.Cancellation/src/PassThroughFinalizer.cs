using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation
{
    /// <summary>
    /// Default finalizer which is used when a cancelable request, has not defined a finalizer.
    /// </summary>
    /// <typeparam name="TRequest">The type of cancelable request being handled.</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    internal class PassThroughFinalizer<TRequest, TResponse> : IResponseFinalizer<TRequest, TResponse>
        where TRequest : ICancelableRequest<TResponse>
    {
        /// <summary>
        /// Simply passes through the current response as it was right before the cancellation.
        /// </summary>
        /// <param name="request">Incoming cancelable request.</param>
        /// <returns>Awaitable task returning the TResponse.</returns>
        public Task<TResponse> Finalize(TRequest request)
        {
            return Task.FromResult(request.Response);
        }
    }
}