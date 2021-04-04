using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public interface IBlobStorageProvider
    {
        Task Upload(Guid tag, byte[] content);
        string GetBlobUrl(Guid tag);
    }

    public class Blob
    {
        public Guid Tag { get; set; }
        public byte[] Content { get; set; }
    }

    public class MockedBlobStorage : IBlobStorageProvider
    {
        private readonly List<Blob> blobs;
        private const string baseUrl = "https://storage.blobprovider.com/";

        public MockedBlobStorage()
        {
            blobs = new List<Blob>();
        }

        public async Task Upload(Guid tag, byte[] content)
        {
            await Task.Delay(200);
            blobs.Add(new Blob()
            {
                Tag = tag,
                Content = content
            });
        }

        public string GetBlobUrl(Guid tag)
        {
            Blob blob = blobs.FirstOrDefault(b => b.Tag == tag);
            return blob != null ? $"{baseUrl}/{blob.Tag}" : string.Empty;
        }
    }
}
