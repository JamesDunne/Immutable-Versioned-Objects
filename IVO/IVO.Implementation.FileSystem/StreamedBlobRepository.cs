using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Errors;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.Diagnostics;
using IVO.Definition;

namespace IVO.Implementation.FileSystem
{
    public sealed class StreamedBlobRepository : IStreamedBlobRepository
    {
        private FileSystem system;

        internal FileSystem FileSystem { get { return system; } }

        /// <summary>
        /// TODO: tune this parameter for best async write buffer size.
        /// </summary>
        private const int largeBufferSize = 1048576;

        public StreamedBlobRepository(FileSystem system)
        {
            this.system = system;
        }

        private async Task<IStreamedBlob> persistBlob(PersistingBlob blob)
        {
            Debug.WriteLine(String.Format("Starting persistence of blob"));

            // Find a temporary filename:
            string objDir = system.getObjectsDirectory().FullName;
            FileInfo tmpPath;
            do
            {
                tmpPath = new FileInfo(Path.Combine(objDir, Path.GetRandomFileName()));
            } while (tmpPath.Exists);

            long length = -1;
            BlobID blid;

            // Open a new stream to the source blob contents:
            using (var sr = blob.Stream)
            {
                length = sr.Length;

                // Create a new file and set its length so we can asynchronously write to it:
                using (var tmpFi = File.Open(tmpPath.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    Debug.WriteLine(String.Format("New BLOB temp '{0}' length {1}", tmpPath.FullName, length));
                    tmpFi.SetLength(length);
                    tmpFi.Close();
                }

                // Determine the best buffer size to use for writing contents:
                int bufSize = Math.Min(Math.Max((int)length, 8), largeBufferSize);

                // Open a new FileStream to asynchronously write the blob contents:
                using (var fs = new FileStream(tmpPath.FullName, FileMode.Open, FileAccess.Write, FileShare.Read, bufSize, useAsync: true))
                using (var sha1 = new SHA1StreamWriter(fs))
                {
                    // Copy the contents asynchronously (expected copy in order):
                    await sr.CopyToAsync(sha1, bufSize).ConfigureAwait(continueOnCapturedContext: false);

                    // Create the BlobID from the SHA1 hash calculated during copy:
                    blid = new BlobID(sha1.GetHash());
                }
            }
            
            // Create the blob's subdirectory under 'objects':
            FileInfo path = system.getPathByID(blid);
            path.Refresh();
            if (!path.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", path.Directory.FullName));
                path.Directory.Create();
            }

            // Don't recreate an existing blob:
            if (path.Exists)
            {
                Debug.WriteLine(String.Format("Blob already exists at path '{0}', deleting...", path.FullName));
                path.Delete();
            }

            // Move the temp file to the final blob filename:
            File.Move(tmpPath.FullName, path.FullName);

            return (IStreamedBlob)new StreamedBlob(this, blid, length);
        }

        public async Task<IStreamedBlob[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            if (blobs == null) throw new ArgumentNullException("blobs");
            if (blobs.Length == 0) return new IStreamedBlob[0];

            // TODO: implement a filesystem lock?

            // Persist each blob to the 'objects' folder asynchronously:
            Task<IStreamedBlob>[] persistTasks = new Task<IStreamedBlob>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                var blob = blobs[i];
                // Start a new task to contain each asynchronous task so that they can start up in parallel with one another:
                persistTasks[i] = TaskEx.RunEx(() => persistBlob(blob));
            }

            Debug.WriteLine("Awaiting all persistence tasks...");
            var streamedBlobs = await TaskEx.WhenAll(persistTasks).ConfigureAwait(continueOnCapturedContext: false);

            Debug.WriteLine("All completed.");

            // Return the final immutable container:
            return streamedBlobs;
        }

        private void deleteBlob(BlobID id)
        {
            FileInfo path = system.getPathByID(id);

            if (path.Exists)
                path.Delete();

            return;
        }

        public Task<BlobID[]> DeleteBlobs(params BlobID[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            if (ids.Length == 0) return TaskEx.FromResult(ids);

            // Delete each blob synchronously:
            for (int i = 0; i < ids.Length; ++i)
            {
                BlobID id = ids[i];
                deleteBlob(id);
            }

            // TODO: Run through all the 'objects' directories and prune empty ones.
            // Too eager? Could cause conflicts with other threads.

            return TaskEx.FromResult(ids);
        }

        private Task<IStreamedBlob> getBlob(BlobID id)
        {
            var fi = system.getPathByID(id);
            if (!fi.Exists) return TaskEx.FromResult( (IStreamedBlob)null );

            return TaskEx.FromResult( (IStreamedBlob)new StreamedBlob(this, id, fi.Length) );
        }

        public Task<IStreamedBlob[]> GetBlobs(params BlobID[] ids)
        {
            Task<IStreamedBlob>[] tasks = new Task<IStreamedBlob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = getBlob(ids[i]);
            return TaskEx.WhenAll(tasks);
        }
    }
}
