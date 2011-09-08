using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO
{
    [TestClass()]
    public class BlobRepositoryTest
    {
        /// <summary>
        ///A test for PersistBlobs
        ///</summary>
        [TestMethod()]
        public void PersistBlobsTest()
        {
            string tmpPath = System.IO.Path.GetTempPath();
            string tmpRoot = System.IO.Path.Combine(tmpPath, "ivo");

            // Delete our temporary 'ivo' folder:
            var tmpdi = new DirectoryInfo(tmpRoot);
            if (tmpdi.Exists)
                tmpdi.Delete(recursive: true);

            FileSystem system = new FileSystem(new DirectoryInfo(tmpRoot));
            IBlobRepository blrepo = new BlobRepository(system);

            Random rnd = new Random(8191);
            
            const int numBlobs = 256;
            Debug.WriteLine("Creating {0} random blobs...", numBlobs);
            
            Blob[] blobArr = new Blob[numBlobs];
            for (int i = 0; i < numBlobs; ++i)
            {
                // Create a random-sized (multiple of 64KB) temp buffer:
                int multiplier = rnd.Next(0, 8) + 12;
                byte[] tmp = new byte[multiplier * 65536];
                rnd.NextBytes(tmp);

                blobArr[i] = new Blob.Builder(tmp);
                tmp = null;
            }

            // Create an immutable container that points to the new blobs:
            var blobs = new ImmutableContainer<BlobID, Blob>(bl => bl.ID, blobArr);

            // Now persist those blobs to the filesystem:
            Debug.WriteLine("Persisting {0} random blobs...", numBlobs);
            Stopwatch sw = Stopwatch.StartNew();
            blrepo.PersistBlobs(blobs).Wait();
            Debug.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, blobArr.Sum(b => b.Contents.Length) * 1000d / sw.ElapsedMilliseconds);
        }
    }
}
