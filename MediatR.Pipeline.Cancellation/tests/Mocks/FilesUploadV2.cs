using System.Collections.Generic;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    /// <summary>
    /// Represents a FilesUploadCommand which supports cancellation and has not defined a IResponseFinalizer.
    /// The builtIn finalizer should be used for this one.
    /// </summary>
    public class FilesUploadCommandV2 : FilesUploadCommand,
         ICancelableRequest<List<UploadResult>>
    {

    }

    public class FilesUploadCommandV2Handler : FilesUploadCommandHandler<FilesUploadCommandV2>,
        IRequestHandler<FilesUploadCommandV2, List<UploadResult>>
    {
        public FilesUploadCommandV2Handler(
            IBlobStorageProvider storageProvider,
            IDatabaseProvider databaseProvider)
            : base(storageProvider, databaseProvider)
        {

        }
    }
}
