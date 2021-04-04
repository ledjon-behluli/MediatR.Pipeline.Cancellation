using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    /// <summary>
    /// Represents a FilesUploadCommand which supports cancellation and has defined a IResponseFinalizer.
    /// </summary>
    public class FilesUploadCommandV1 : FilesUploadCommand,
         ICancelableRequest<List<UploadResult>>
    {

    }

    public class FilesUploadCommandV1Handler : FilesUploadCommandHandler<FilesUploadCommandV1>,
        IRequestHandler<FilesUploadCommandV1, List<UploadResult>>
    {
        public FilesUploadCommandV1Handler(
            IBlobStorageProvider storageProvider,
            IDatabaseProvider databaseProvider)
            : base(storageProvider, databaseProvider)
        {
           
        }
    }

    /// <summary>
    /// Custom finalizer for FilesUploadCommandV1
    /// </summary>
    public class FilesUploadCommandV1Finalizer : IResponseFinalizer<FilesUploadCommandV1, List<UploadResult>>
    {
        public async Task<List<UploadResult>> Finalize(FilesUploadCommandV1 request)
        {
            await Task.Delay(1);    // Might be a longer running finalization

            foreach (var result in request.Response)
            {
                if (result.UploadStatus == UploadResult.Status.Pending)
                {
                    result.SetStatus(UploadResult.Status.Canceled);
                    result.ModifiedByFinalizer = true;
                }
            }

            return request.Response;
        }
    }
}
