using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public class File
    {
        public Guid Tag { get; set; }
        public byte[] Content { get; set; }
    }

    public abstract class FilesUploadCommand
    { 
        public List<File> Files = new List<File>();

        public ICounter Counter { get; set; }       // Used for testing only!
        public List<UploadResult> Response { get; } = new List<UploadResult>();
    }

    public abstract class FilesUploadCommandHandler<T> where T : FilesUploadCommand
    {
        private readonly IBlobStorageProvider storageProvider;
        private readonly IDatabaseProvider databaseProvider;
        
        public FilesUploadCommandHandler(
            IBlobStorageProvider storageProvider,
            IDatabaseProvider databaseProvider)
        {
            this.storageProvider = storageProvider;
            this.databaseProvider = databaseProvider;
        }

        public async Task<List<UploadResult>> Handle(T request, CancellationToken cancellationToken)
        {
            int count = 0;
            request.Files.ForEach(f =>
            {
                request.Response.Add(new UploadResult(f.Tag));
            });

            foreach (var file in request.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await storageProvider.Upload(file.Tag, file.Content);
                await databaseProvider.Persist(new BlobEntity() { Tag = file.Tag });

                var response = request.Response.Find(r => r.Tag == file.Tag);

                response.SetUrl(storageProvider.GetBlobUrl(file.Tag));
                response.SetStatus(UploadResult.Status.Succeeded);

                count++;
                request.Counter?.Invoke();
            }

            return request.Response;
        }
    }
}
