using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MediatR.Pipeline.Cancellation.Tests.Mocks
{
    public static class Generator
    {
        public static List<File> RandomFiles(int count = 1)
        {
            var files = new List<File>();

            for (int i = 0; i < count; i++)
            {
                files.Add(new File()
                {
                    Tag = Guid.NewGuid(),
                    Content = GenerateRandomData()
                });
            }

            return files;
        }

        public static ICounter GetCounter(int cancelOnCount, out CancellationTokenSource tokenSource)
        {
            int count = 0;
            CancellationTokenSource cts = new CancellationTokenSource();
            var mock = new Mock<ICounter>();

            mock.Setup(x => x.Invoke())
                .Callback(() =>
                {
                    count++;
                    if (count == cancelOnCount)
                    {
                        cts.Cancel();
                    }
                });

            tokenSource = cts;
            return mock.Object;
        }

        private static byte[] GenerateRandomData(int length = 256)
        {
            Random rand = new Random();
            var buffer = new byte[length];
            rand.NextBytes(buffer);
            return buffer;
        }
    }
}
