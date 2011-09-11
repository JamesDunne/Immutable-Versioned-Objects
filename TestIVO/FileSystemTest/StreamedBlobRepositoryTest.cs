using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Security.Cryptography;
using IVO.Definition;
using IVO.TestSupport;

namespace TestIVO.FileSystemTest
{
    [TestClass()]
    public class StreamedBlobRepositoryTest
    {
        private FileSystem getFileSystem()
        {
            string tmpPath = System.IO.Path.GetTempPath();
            string tmpRoot = System.IO.Path.Combine(tmpPath, "ivo");

            // Delete our temporary 'ivo' folder:
            var tmpdi = new DirectoryInfo(tmpRoot);
            if (tmpdi.Exists)
                tmpdi.Delete(recursive: true);

            FileSystem system = new FileSystem(new DirectoryInfo(tmpRoot));
            return system;
        }

        [TestMethod()]
        public void PersistBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).PersistBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        private PersistingBlob[] createBlobs(int numBlobs)
        {
            PersistingBlob[] blobs = new PersistingBlob[numBlobs];
            for (int i = 0; i < numBlobs; ++i)
            {
                string tmpFileName = System.IO.Path.GetTempFileName();
                using (var fs = new FileStream(tmpFileName, FileMode.Open, FileAccess.Write, FileShare.Read, 1048576, true))
                {
                    new RandomDataStream(1048576 * 2).CopyTo(fs);

                    blobs[i] = new PersistingBlob(() => new FileStream(tmpFileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                }
            }
            return blobs;
        }

        [TestMethod()]
        public void PersistBlobsTest2()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            const int numBlobs = 16;

            TaskEx.Run(async () => {
                // Create an immutable container that points to the new blobs:
                Console.WriteLine("Creating {0} random blobs...", numBlobs);
                PersistingBlob[] blobs = createBlobs(numBlobs);

                // Now persist those blobs to the filesystem:
                Console.WriteLine("Persisting {0} random blobs...", numBlobs);
                Stopwatch sw = Stopwatch.StartNew();
                var streamedBlobs = await blrepo.PersistBlobs(blobs);
                sw.Stop();

                Console.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, streamedBlobs.Sum(b => b.Length) * 1000d / sw.ElapsedMilliseconds);

                // Get the blobs:
                var getBlobs = await blrepo.GetBlobs(blobs.Select(bl => bl.ComputedID).ToArray(blobs.Length));
                for (int i = 0; i < getBlobs.Length; ++i)
                {
                    Console.WriteLine("{0}", getBlobs[i].ID);
                }
            }).Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).DeleteBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod()]
        public void GetBlobsTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).GetBlobsTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            FileSystem system = getFileSystem();
            IStreamedBlobRepository blrepo = new StreamedBlobRepository(system);

            new CommonTest.StreamedBlobRepositoryTestMethods(blrepo).StreamedBlobTest().Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }
    }
}
