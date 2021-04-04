using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public class BlobEntity
    {
        public Guid Tag { get; set; }
    }

    public interface IDatabaseProvider
    {
        Task Persist(BlobEntity blob);
    }

    public class MockedDatabase : IDatabaseProvider
    {
        private readonly List<BlobEntity> blobs;

        public MockedDatabase()
        {
            blobs = new List<BlobEntity>();
        }

        public async Task Persist(BlobEntity blob)
        {
            await Task.Delay(50);
            blobs.Add(blob);
        }
    }
}
