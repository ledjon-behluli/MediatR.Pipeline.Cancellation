![alt text](https://github.com/ledjon-behluli/MediatR.Pipeline.Cancellation/blob/master/MediatR.Pipeline.Cancellation/icon.png?raw=true)
# MediatR.Pipeline.Cancellation
MediatR pipeline that handles canceled requests gracefully. 

Library is available through [NuGet](https://www.nuget.org/packages/MediatR.Pipeline.Cancellation/).

## Dependency Injection

You will need to register the cancellation pipeline along with all implementations of `IResponseFinalizer<TRequest, TResponse>`.

#### BuiltIn Registration

The library has a convenient registration method that you can use.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCancellationPipeline(Assembly.GetExecutingAssembly());
}
```

#### Manual Registration

If you prefer you can manually register the pipeline and the finalizers using various IoC containers.

Microsoft's builtIn container:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CancelableRequestBehavior<,>));
    services.AddTransient<IResponseFinalizer<Hello, string>, HelloFinalizer>();
}
```

Autofac container:

```csharp
protected override void Load(ContainerBuilder builder)
{
     builder.RegisterGeneric(typeof(CancelableRequestBehavior<,>))
            .As(typeof(IPipelineBehavior<,>));
            
     builder.RegisterAssemblyTypes(ThisAssembly)
            .AsClosedTypesOf(typeof(IResponseFinalizer<,>));
}
```

## Hello-World
### Scenario
Send a request and pass in your name, the response has a `string` return type, which will be enhanced by the handler.

### Example
Let's create a request called `Hello`.

`Hello` implements `ICancelableRequest<string>`'s `TResponse Response { get; }` property, which means `TResponse` will be of type `string`.
```csharp
public class Hello : ICancelableRequest<string>
{
    public string YourName { get; set; }
    public string Response { get; set; }
}
```

Let's create a handler for `Hello` called `World`, which populates the `request.Response` object with some information.
```csharp
public class World : IRequestHandler<Hello, string>
{
    public async Task<string> Handle(Hello request, CancellationToken cancellationToken)
    {
        request.Response = $"Hello {request.YourName} - Im in the handler now";

        await Task.Delay(100);
        cancellationToken.ThrowIfCancellationRequested();

        request.Response += " - Im leaving the handler now";
        return request.Response;
    }
}
```
Let's also create a concrete implementation of `IResponseFinalizer` called `HelloFinalizer`.
It's `Finalize(request)` method takes as an argument the original request, which in turn has the response object we are building.
```csharp
public class HelloFinalizer : IResponseFinalizer<Hello, string>
{
    public async Task<string> Finalize(Hello request)
    {
        request.Response += " - Im in the finalizer now";
        await Task.Delay(100);
        return request.Response + " - Im leaving the finalizer now";
    }
}
```
If we run this example and cancel the request, the test should pass.
```csharp
CancellationTokenSource cts = new CancellationTokenSource();
cts.CancelAfter(50);

var result = await mediator.Send(new Hello()
{
    YourName = "John"
}, cts.Token);

Assert.Equal("Hello John - Im in the handler now - Im in the finalizer now - Im leaving the finalizer now", result);
```
If run this example and this time we don't cancel the request, the test should pass.
```csharp
var result = await mediator.Send(new Hello()
{
    YourName = "John"
});

Assert.Equal("Hello John - Im in the handler now - Im leaving the handler now", result);
```

*Note: If we did not implement a finalizer for this request, than the pipeline will use an internal pass-through finalizer, which will simply handle the cancellation and return the response object as it was right before the cancellation.*

This test should pass.

```csharp
Assert.Equal("Hello John - Im in the handler now", result);
```

## Ping-Pong
### Scenario
A lot of times our request don't return a response at all. But we still want to be able to handle the cancellation gracefully.

For these scenarios the library offers an abstract class called `CancelableRequest` which your request object can inherit from. The return type is implicitly declared as void (or `Unit` since this is an extension for MediatR).

### Example
Let's create a request called `Ping`.

`Ping` inherits from `CancelableRequest` so it doesn't have to implement the `TResponse Response { get; }` property, because it returns `Unit`.
```csharp
public class Ping : CancelableRequest
{
   
}
```
Let's create a handler for `Ping` called `Pong`.
```csharp
public class Pong : IRequestHandler<Ping, Unit>
{
    public async Task<Unit> Handle(Ping request, CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(50);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
```
Let's also create a concrete implementation of `IResponseFinalizer` called `PingFinalizer`.
It's `Finalize(request)` method takes as an argument the original request as it has to implement the interface, but simply returns `Unit.Value`.

*Note: This finalizer isn't required as the response is void. The pass-through finalizer could handle it, but we might want to do more than just return void, we could log the canceled action, call a webservice etc.*
```csharp
public class PingFinalizer : IResponseFinalizer<Ping>
{
    public async Task<Unit> Finalize(Ping request)
    {
        await Task.Delay(10); // Do some finalization work (maybe log the action, call a webservice etc).
        return Unit.Value;
    }
}
```
If we run this example and cancel the request, the test should pass.
```csharp
CancellationTokenSource cts = new CancellationTokenSource();
cts.CancelAfter(500);

var exception = await Record.ExceptionAsync(() =>
    mediator.Send(new Ping(), cts.Token));

Assert.Null(exception);
```

## Multi-File Upload
### Scenario
Quite common applications support uploading multiple files. The file size obviously can vary which in turn means the time it takes to upload also varies quite a bit.

Being able to cancel the request but also returning which files have finished uploading and which have been canceled, provides a good user experience. 

*Note: Canceling some operations midway through, may result in corruption and/or incorrect data. In these cases it is critical to do some finalization work to ensure correctness.* 

### Example
Let's create a request called `FilesUploadCommand`. It takes a collection of `File` objects to upload, and returns a collection of `UploadResult` objects.
```csharp
public class File
{
    public Guid Tag { get; set; }
    public byte[] Content { get; set; }
}

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
```

`FilesUploadCommand` implements `ICancelableRequest<List<UploadResult>>`'s `TResponse Response { get; }` property, which means `TResponse` will be of type `List<UploadResult>`.

```csharp
public class FilesUploadCommand : ICancelableRequest<List<UploadResult>>
{
	public List<File> Files = new List<File>();
	public ICounter Counter { get; set; }       // Used for testing only!
	public List<UploadResult> Response { get; } = new List<UploadResult>();
}
```

The `ICounter` has a single method `Invoke` which is used to tell the test that a file has finished uploading. This will be used for our conditional cancellation.

```csharp
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
```

Let's create a handler for `FilesUploadCommand`, which populates the response object with some information.

```csharp
public class FilesUploadCommandHandler : IRequestHandler<FilesUploadCommand, List<UploadResult>>
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
```
The `IBlobStorageProvider` & `IDatabaseProvider` are abstractions for uploading the file to a blob storage and storing the tag in a database.

Let's also create a concrete implementation of `IResponseFinalizer` called `FilesUploadCommandFinalizer`.
```csharp
public class FilesUploadCommandFinalizer : IResponseFinalizer<FilesUploadCommand, List<UploadResult>>
{
    public async Task<List<UploadResult>> Finalize(FilesUploadCommand request)
    {
        await Task.Delay(1);    // Might be an anti-corruption mechanizm work.

        foreach (var result in request.Response)
        {
	        // If this UploadStatus is 'Pending' than set it to 'Canceled'.
	        // Also mark it as modified by finalizer.
            if (result.UploadStatus == UploadResult.Status.Pending)
            {
                result.SetStatus(UploadResult.Status.Canceled);
                result.ModifiedByFinalizer = true;	// Used for testing only!
            }
        }

        return request.Response;
    }
}
```
We tell the [Generator](https://github.com/ledjon-behluli/MediatR.Pipeline.Cancellation/blob/master/MediatR.Pipeline.Cancellation/tests/Mocks/Generator.cs) to produce 4 random files and provides us with a `CancellationTokenSource` which should cancel the request after 2 uploads.

If we run this example, the last 2 `UploadResult`'s should have `UploadResult.Status.Canceled` &  `UploadResult.ModifiedByFinalizer` flag should be true.
```csharp
var results = await mediator.Send(new FilesUploadCommand()      
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
Assert.Collection(results,
    r => Assert.False(r.ModifiedByFinalizer),
    r => Assert.False(r.ModifiedByFinalizer),
    r => Assert.True(r.ModifiedByFinalizer),
    r => Assert.True(r.ModifiedByFinalizer)
);
```

If run this example, and this time we don’t cancel the request, the result should not contain any `UploadResult.Status.Pending` &  `UploadResult.ModifiedByFinalizer` flag should be false.
```csharp
var results = await mediator.Send(new FilesUploadCommand()
{
    Files = Generator.RandomFiles(4)
});

Assert.DoesNotContain(results, r => r.UploadStatus == UploadResult.Status.Pending);
Assert.DoesNotContain(results, r => r.ModifiedByFinalizer);
```
---

If you find this library helpful, please consider giving it a ✰ and share it!
You are free to modify it, under the condition of including the link to the original author.

Copyright © 2021 Ledjon Behluli
