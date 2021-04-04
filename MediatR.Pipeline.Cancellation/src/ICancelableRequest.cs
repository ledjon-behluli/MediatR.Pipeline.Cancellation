namespace MediatR.Pipeline.Cancellation
{
    /// <summary>
    /// Marker interface to represent a cancelable request with a response.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface ICancelableRequest<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// The response under action.
        /// Eventually this will be returned or finalized and than returned, if the request has been canceled.
        /// </summary>
        TResponse Response { get; }
    }

    /// <summary>
    /// Implementor of <see cref="ICancelableRequest{TResponse}"/> where TResponse is void.
    /// </summary>
    public abstract class CancelableRequest : ICancelableRequest<Unit>
    {
        /// <summary>
        /// Unit response.
        /// </summary>
        public Unit Response => Unit.Value;
    }
}
