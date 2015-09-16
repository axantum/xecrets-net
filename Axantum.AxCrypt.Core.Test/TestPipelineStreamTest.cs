using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestPipelineStreamTest
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestSimpleByte()
        {
            byte[] result = null;
            bool complete = false;

            using (PipelineStream pipeline = new PipelineStream(new CancellationToken()))
            {
                Task producerTask = Task.Run(() =>
                {
                    pipeline.WriteByte(13);
                    pipeline.Complete();
                });

                Task consumerTask = Task.Run(() =>
                {
                    byte[] r = new byte[1];
                    r[0] = (byte)pipeline.ReadByte();
                    result = r;
                    complete = pipeline.ReadByte() == -1;
                });

                Task.WaitAll(producerTask, consumerTask);
            }

            Assert.That(result[0], Is.EqualTo(13), "The byte written and read has the value 13.");
            Assert.That(complete, Is.True, "Since only one byte was written, complete should be set as well.");
        }

        [Test]
        public void TestLargeWithVaryingChunkSizes()
        {
            FakeRandomGenerator frg = new FakeRandomGenerator();

            byte[] source = frg.Generate(500000);

            int[] chunkSizes = { 1, 2, 3, 5, 7, 11, 13, 17, 19, 64, 128, 256, 257, };

            using (MemoryStream destination = new MemoryStream())
            {
                using (PipelineStream pipeline = new PipelineStream(new CancellationToken()))
                {
                    Task producerTask = Task.Run(() =>
                    {
                        int i = 0;
                        int total = 0;
                        while (total < source.Length)
                        {
                            int count = chunkSizes[i++ % chunkSizes.Length];
                            count = total + count > source.Length ? source.Length - total : count;
                            pipeline.Write(source, total, count);
                            total += count;
                        }
                        pipeline.Complete();
                    });

                    Task consumerTask = Task.Run(() =>
                    {
                        byte[] read = new byte[17];
                        int count;
                        while ((count = pipeline.Read(read, 0, read.Length)) > 0)
                        {
                            destination.Write(read, 0, count);
                        }
                    });

                    Task.WaitAll(producerTask, consumerTask);
                }

                byte[] result = destination.ToArray();

                Assert.That(source.IsEquivalentTo(result), "The source and result should be the same after passing through the pipeline.");
            }
        }

        [Test]
        public void TestLargeStreamCopy()
        {
            FakeRandomGenerator frg = new FakeRandomGenerator();

            using (MemoryStream source = new MemoryStream(frg.Generate(1000000)))
            {
                using (MemoryStream destination = new MemoryStream())
                {
                    using (PipelineStream pipeline = new PipelineStream(new CancellationToken()))
                    {
                        Task producerTask = Task.Run(() =>
                        {
                            source.CopyTo(pipeline);
                            pipeline.Complete();
                        });

                        Task consumerTask = Task.Run(() =>
                        {
                            pipeline.CopyTo(destination);
                        });

                        Task.WaitAll(producerTask, consumerTask);
                    }

                    Assert.That(source.ToArray().IsEquivalentTo(destination.ToArray()), "The source and destination should be the same after passing through the pipeline.");
                }
            }
        }
    }
}