using System.Collections.Generic;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    /// <summary>
    /// Represents a FilesUploadCommand which doesn't support cancellation.
    /// </summary>
    public class FilesUploadCommandV3 : FilesUploadCommand,
         IRequest<List<UploadResult>>
    {

    }

    public class FilesUploadCommandV3Handler : FilesUploadCommandHandler<FilesUploadCommandV3>,
        IRequestHandler<FilesUploadCommandV3, List<UploadResult>>
    {
        public FilesUploadCommandV3Handler(
            IBlobStorageProvider storageProvider,
            IDatabaseProvider databaseProvider)
            : base(storageProvider, databaseProvider)
        {

        }
    }
}
