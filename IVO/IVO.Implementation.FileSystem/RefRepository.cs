using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;
using System.Diagnostics;
using IVO.Definition.Errors;

namespace IVO.Implementation.FileSystem
{
    public sealed class RefRepository : IRefRepository
    {
        private FileSystem system;

        public RefRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private void persistRef(Ref rf)
        {
            FileInfo fi = system.getRefPathByRefName(rf.Name);

            // Create directory if it doesn't exist:
            if (!fi.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                fi.Directory.Create();
            }

            // Write the contents to the file:
            using (var fs = new FileStream(fi.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Debug.WriteLine(String.Format("New REF '{0}'", fi.FullName));
                rf.WriteTo(fs);
            }
        }

        private async Task<Errorable<Ref>> getRefByName(RefName refName)
        {
            FileInfo fiTracker = system.getRefPathByRefName(refName);
            if (!fiTracker.Exists) return new RefNameDoesNotExistError(refName);

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fiTracker.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384).ConfigureAwait(continueOnCapturedContext: false);
                if (nr >= 16384)
                {
                    // My, what a large tag you have!
                    throw new NotSupportedException();
                }
            }

            // Parse the CommitID:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line = sr.ReadLine();
                if (line == null) return new RefNameDoesNotExistError(refName);

                var ecid = CommitID.TryParse(line);
                if (ecid.HasErrors) return ecid.Errors;

                return (Ref) new Ref.Builder(refName, ecid.Value);
            }
        }

        private void deleteRef(RefName name)
        {
            FileInfo fi = system.getRefPathByRefName(name);
            if (fi.Exists) fi.Delete();
        }

        #endregion

        public Task<Errorable<Ref>> PersistRef(Ref rf)
        {
            persistRef(rf);
            return TaskEx.FromResult((Errorable<Ref>)rf);
        }

        public Task<Errorable<Ref>> GetRefByName(RefName name)
        {
            return getRefByName(name);
        }

        public async Task<Errorable<Ref>> DeleteRefByName(RefName name)
        {
            var erf = await getRefByName(name).ConfigureAwait(continueOnCapturedContext: false);
            if (erf.HasErrors) return erf.Errors;

            deleteRef(name);
            
            return erf.Value;
        }
    }
}
