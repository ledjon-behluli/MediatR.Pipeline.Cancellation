using MediatR.Pipeline.Cancellation.Tests.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Pipeline.Cancellation.Tests
{
    public class PipelineTests
    {
        private readonly IMediator mediator;

        public PipelineTests(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Fact]
        public async Task Successfull_Upload_Should_Result_In_Same_Number_Of_UploadResponse_For_Files_Sent()
        {
            var results = await mediator.Send(new FilesUploadCommandV1()
            {
                Files = Generator.RandomFiles(4)
            });

            Assert.Equal(4, results.Count);
        }

        [Fact]
        public async Task Successfull_Upload_Should_Not_Result_In_Any_UploadResponse_With_Pending_Status()
        {
            var results = await mediator.Send(new FilesUploadCommandV1()
            {
                Files = Generator.RandomFiles(4)
            });

            Assert.DoesNotContain(results, r => r.UploadStatus == UploadResult.Status.Pending);
        }

        [Fact]
        public async Task Successfull_Upload_Should_Not_Result_In_Any_UploadRespons_With_ModifiedByFinalizer_Flag_Being_True()
        {
            var results = await mediator.Send(new FilesUploadCommandV1()
            {
                Files = Generator.RandomFiles(4)
            });

            Assert.DoesNotContain(results, r => r.ModifiedByFinalizer);
        }

        [Fact]
        public async Task Canceled_Upload_Should_Not_Result_In_Any_UploadResponse_With_Pending_Status()
        {
            var results = await mediator.Send(new FilesUploadCommandV1()
            {
                Files = Generator.RandomFiles(4),
                Counter = Generator.GetCounter(2, out CancellationTokenSource cts)
            }, cts.Token);

            Assert.DoesNotContain(results, r => r.UploadStatus == UploadResult.Status.Pending);
        }

        [Fact]
        public async Task Canceled_Upload_Should_Result_In_The_Last_Two_UploadResults_With_Canceled_Status_For_Request_With_A_Defined_Finalizer()
        {
            // V1 defines a custom IResponseFinalizer
            var results = await mediator.Send(new FilesUploadCommandV1()      
            {
                Files = Generator.RandomFiles(4),
                Counter = Generator.GetCounter(cancelOnCount: 2, out CancellationTokenSource cts)
            }, cts.Token);

            Assert.Collection(results, 
                r => Assert.Equal(UploadResult.Status.Succeeded, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Succeeded, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Canceled, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Canceled, r.UploadStatus)
            );
        }

        [Fact]
        public async Task Canceled_Upload_Should_Result_In_The_Last_Two_UploadResults_With_Pending_Status_For_Request_Without_A_Finalizer()
        {
            // V2 doesnt define a custom IResponseFinalizer, the pipeline should use the builtIn 'PassThroughFinalizer'
            var results = await mediator.Send(new FilesUploadCommandV2()
            {
                Files = Generator.RandomFiles(4),
                Counter = Generator.GetCounter(cancelOnCount: 2, out CancellationTokenSource cts)
            }, cts.Token);

            Assert.Collection(results,
                r => Assert.Equal(UploadResult.Status.Succeeded, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Succeeded, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Pending, r.UploadStatus),
                r => Assert.Equal(UploadResult.Status.Pending, r.UploadStatus)
            );
        }


        [Fact]
        public async Task Canceled_Upload_Should_Result_In_The_Last_Two_UploadResults_With_ModifiedByFinalizer_Flag_Being_True_For_Request_With_A_Defined_Finalizer()
        {
            // V1 defines a custom IResponseFinalizer
            var results = await mediator.Send(new FilesUploadCommandV1()
            {
                Files = Generator.RandomFiles(4),
                Counter = Generator.GetCounter(cancelOnCount: 2, out CancellationTokenSource cts)
            }, cts.Token);

            Assert.Collection(results,
                r => Assert.False(r.ModifiedByFinalizer),
                r => Assert.False(r.ModifiedByFinalizer),
                r => Assert.True(r.ModifiedByFinalizer),
                r => Assert.True(r.ModifiedByFinalizer)
            );
        }

        [Fact]
        public async Task Canceled_Upload_Should_Result_In_All_ModifiedByFinalizer_Flags_Being_False_For_Request_Without_A_Finalizer()
        {
            // V2 doesnt define a custom IResponseFinalizer, the pipeline should use the builtIn 'PassThroughFinalizer'
            var results = await mediator.Send(new FilesUploadCommandV2()
            {
                Files = Generator.RandomFiles(4),
                Counter = Generator.GetCounter(cancelOnCount: 2, out CancellationTokenSource cts)
            }, cts.Token);

            Assert.Collection(results,
                r => Assert.False(r.ModifiedByFinalizer),
                r => Assert.False(r.ModifiedByFinalizer),
                r => Assert.False(r.ModifiedByFinalizer),
                r => Assert.False(r.ModifiedByFinalizer)
            );
        }

        [Fact]
        public async Task Pipeline_Should_Handle_Request_Which_Is_Cancelable__Is_Canceled__And_Has_A_Finalizer_Defined()
        {
            // V1 is cancelable and defines a custom IResponseFinalizer
            var exception = await Record.ExceptionAsync(() =>
                mediator.Send(new FilesUploadCommandV1()
                {
                    Files = Generator.RandomFiles(4),
                    Counter = Generator.GetCounter(2, out CancellationTokenSource cts)
                }, cts.Token
            ));

            Assert.Null(exception);
        }

        [Fact]
        public async Task Pipeline_Should_Handle_Request_Which_Is_Cancelable__Is_Canceled__And_Has_Not_A_Defined_Finalizer()
        {
            // V2 is cancelable and doesn't define a custom IResponseFinalizer, the pipeline should use the builtIn 'PassThroughFinalizer'
            var exception = await Record.ExceptionAsync(() =>
                mediator.Send(new FilesUploadCommandV2()
                {
                    Files = Generator.RandomFiles(4),
                    Counter = Generator.GetCounter(2, out CancellationTokenSource cts)
                }, cts.Token
            ));

            Assert.Null(exception);
        }

        [Fact]
        public async Task Pipeline_Should_Not_Handle_Requests_Which_Are_Not_Cancelable_And_Are_Canceled()
        {
            // V3 is not cancelable, the pipeline should not handle this one.
            var exception = await Record.ExceptionAsync(() =>
                mediator.Send(new FilesUploadCommandV3()
                {
                    Files = Generator.RandomFiles(4),
                    Counter = Generator.GetCounter(2, out CancellationTokenSource cts)
                }, cts.Token
            ));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task Pipeline_Should_Handle_Request_Which_Is_Canceled_And_Has_Void_Response()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(500);

            var exception = await Record.ExceptionAsync(() =>
                mediator.Send(new Ping(), cts.Token));

            Assert.Null(exception);
        }

        [Fact]
        public async Task Response_Should_Come_From_Handler_If_Request_Is_Not_Canceled()
        {
            var result = await mediator.Send(new Hello()
            {
                YourName = "John"
            });

            Assert.Equal("Hello John - Im in the handler now - Im leaving the handler now", result);
        }

        [Fact]
        public async Task Response_Should_Come_From_Finalizer_If_Request_Is_Canceled()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            var result = await mediator.Send(new Hello()
            {
                YourName = "John"
            }, cts.Token);

            Assert.Equal("Hello John - Im in the handler now - Im in the finalizer now - Im leaving the finalizer now", result);
        }
    }
}
