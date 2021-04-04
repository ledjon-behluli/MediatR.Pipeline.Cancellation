using System;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public class UploadResult
    {
        public enum Status
        {
            Pending,
            Succeeded,
            Canceled
        }

        public Guid Tag { get; }
        public string ContentUrl { get; private set; }
        public Status UploadStatus { get; private set; }
        public bool ModifiedByFinalizer { get; set; }

        public UploadResult(Guid tag)
        {
            Tag = tag;
            UploadStatus = Status.Pending;
            ModifiedByFinalizer = false;
        }

        public void SetUrl(string url) => ContentUrl = url;
        public void SetStatus(Status status) => UploadStatus = status;
    }
}
