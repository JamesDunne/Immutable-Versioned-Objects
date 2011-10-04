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

        private async Task<Errorable<IStreamedBlob>> persistBlob(PersistingBlob blob)
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

            // Serialize
            lock (FileSystem.SystemLock)
            {
                path.Refresh();
                if (!path.Directory.Exists)
                {
                    Debug.WriteLine(String.Format("New DIR '{0}'", path.Directory.FullName));
                    path.Directory.Create();
                }

                // Don't recreate an existing blob:
                if (path.Exists)
                {
                    Debug.WriteLine(String.Format("Blob already exists at path '{0}', deleting temporary...", path.FullName));
                    tmpPath.Delete();
                    return new Errorable<IStreamedBlob>((IStreamedBlob)new StreamedBlob(this, blid, length));
                }

                // Move the temp file to the final blob filename:
                File.Move(tmpPath.FullName, path.FullName);
            }

            return new Errorable<IStreamedBlob>((IStreamedBlob)new StreamedBlob(this, blid, length));
        }

        public async Task<Errorable<IStreamedBlob>[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            if (blobs == null) throw new ArgumentNullException("blobs");
            if (blobs.Length == 0) return new Errorable<IStreamedBlob>[0];

            // TODO: implement a filesystem lock?

            // Persist each blob to the 'objects' folder asynchronously:
            Task<Errorable<IStreamedBlob>>[] persistTasks = new Task<Errorable<IStreamedBlob>>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                var blob = blobs[i];
                // Start a new task to contain each asynchronous task so that they can start up in parallel with one another:
                persistTasks[i] = persistBlob(blob);
            }

            Debug.WriteLine("Awaiting all persistence tasks...");
            var streamedBlobs = await TaskEx.WhenAll(persistTasks).ConfigureAwait(continueOnCapturedContext: false);

            Debug.WriteLine("All completed.");

            // Return the final immutable container:
            return streamedBlobs;
        }

        private Errorable<BlobID> deleteBlob(BlobID id)
        {
            FileInfo path = system.getPathByID(id);

            lock (FileSystem.SystemLock)
            {
                if (!path.Exists) return new BlobIDRecordDoesNotExistError(id);

                path.Delete();
            }

            return id;
        }

        public Task<Errorable<BlobID>[]> DeleteBlobs(params BlobID[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            if (ids.Length == 0) return TaskEx.FromResult(new Errorable<BlobID>[0]);

            // Delete each blob synchronously:
            Errorable<BlobID>[] results = new Errorable<BlobID>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                BlobID id = ids[i];
                results[i] = deleteBlob(id);
            }

            // TODO: Run through all the 'objects' directories and prune empty ones.
            // Too eager? Could cause conflicts with other threads.

            return TaskEx.FromResult(results);
        }

        private Task<Errorable<IStreamedBlob>> getBlob(BlobID id)
        {
            var fi = system.getPathByID(id);
            if (!fi.Exists) return TaskEx.FromResult((Errorable<IStreamedBlob>)new BlobIDRecordDoesNotExistError(id));

            return TaskEx.FromResult(new Errorable<IStreamedBlob>((IStreamedBlob)new StreamedBlob(this, id, fi.Length)));
        }

        public Task<Errorable<IStreamedBlob>[]> GetBlobs(params BlobID[] ids)
        {
            Task<Errorable<IStreamedBlob>>[] tasks = new Task<Errorable<IStreamedBlob>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = getBlob(ids[i]);
            return TaskEx.WhenAll(tasks);
        }

        public Task<Errorable<IStreamedBlob>> PersistBlob(PersistingBlob blob)
        {
            return persistBlob(blob);
        }

        public Task<Errorable<BlobID>> DeleteBlob(BlobID id)
        {
            return TaskEx.FromResult(deleteBlob(id));
        }

        public Task<Errorable<IStreamedBlob>> GetBlob(BlobID id)
        {
            return getBlob(id);
        }

        public Task<Errorable<BlobID>> ResolvePartialID(BlobID.Partial id)
        {
            FileInfo[] fis = system.getPathsByPartialID(id);
            if (fis.Length == 1) return TaskEx.FromResult( BlobID.TryParse(id.ToString().Substring(0, 2) + fis[0].Name) );
            if (fis.Length == 0) return TaskEx.FromResult( (Errorable<BlobID>) new BlobIDPartialNoResolutionError(id) );
            return TaskEx.FromResult( (Errorable<BlobID>) new BlobIDPartialAmbiguousResolutionError(id, fis.SelectAsArray(f => BlobID.TryParse(id.ToString().Substring(0, 2) + f.Name).Value)) );
        }

        public Task<Errorable<BlobID>[]> ResolvePartialIDs(params BlobID.Partial[] ids)
        {
            return TaskEx.WhenAll( ids.SelectAsArray(id => ResolvePartialID(id)) );
        }
    }
}
