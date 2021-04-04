
namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public interface ICounter
    {
        void Invoke();
    }

    public class Counter : ICounter
    {
        public void Invoke()
        {
           
        }
    }
}
