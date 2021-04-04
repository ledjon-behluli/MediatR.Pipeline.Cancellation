using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation
{
    /// <summary>
    /// Marker interface to represent a finalizer of a cancelable request with a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of cancelable request being handled.</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    public interface IResponseFinalizer<in TRequest, TResponse>
        where TRequest : ICancelableRequest<TResponse>
    {
        /// <summary>
        /// Applies finalization to a cancelable request.
        /// </summary>
        /// <param name="request">The cancelable request.</param>
        /// <returns>Finalized response.</returns>
        Task<TResponse> Finalize(TRequest request);
    }

    /// <summary>
    /// Marker interface to represent a finalizer of a cancelable request with a void response.
    /// </summary>
    /// <typeparam name="TRequest">The type of cancelable request being handled.</typeparam>
    public interface IResponseFinalizer<in TRequest> : IResponseFinalizer<TRequest, Unit>
        where TRequest : CancelableRequest
    {
       
    }
}
