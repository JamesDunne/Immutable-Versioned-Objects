using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Errors;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;
using System.Diagnostics;

namespace IVO.Implementation.FileSystem
{
    public sealed class StageRepository : IStageRepository
    {
        private FileSystem system;

        public StageRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private async Task<Errorable<Stage>> persistStage(Stage stg)
        {
            FileInfo tmpFile = system.getTemporaryFile();
            using (var fs = new FileStream(tmpFile.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: 16386, useAsync: true))
            {
                await fs.WriteRawAsync(stg.WriteTo(new StringBuilder()).ToString());
            }

            lock (FileSystem.SystemLock)
            {
                FileInfo fi = system.getStagePathByStageName(stg.Name);
                if (fi.Exists)
                    fi.Delete();

                // Create directory if it doesn't exist:
                if (!fi.Directory.Exists)
                {
                    Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                    fi.Directory.Create();
                }

                // Write the contents to the file:
                Debug.WriteLine(String.Format("New STAGE '{0}'", fi.FullName));
                File.Move(tmpFile.FullName, fi.FullName);
            }

            return stg;
        }

        private async Task<Errorable<Stage>> getStageByName(StageName stageName)
        {
            FileInfo fiTracker = system.getStagePathByStageName(stageName);
            if (!fiTracker.Exists) return new StageNameDoesNotExistError(stageName);

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

            Stage.Builder sb = new Stage.Builder();

            // Parse the Stage:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line = sr.ReadLine();

                // Set Name:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("name ")) return new StageParseExpectedNameError();
                sb.Name = (StageName)line.Substring("name ".Length);

                // Set TreeID:
                if (line == null || !line.StartsWith("tree ")) return new StageParseExpectedTreeError();
                var ecid = TreeID.TryParse(line.Substring("tree ".Length));
                if (ecid.HasErrors) return ecid.Errors;
                sb.TreeID = ecid.Value;

                return (Stage)sb;
            }
        }

        private void deleteStage(StageName name)
        {
            lock (FileSystem.SystemLock)
            {
                FileInfo fi = system.getStagePathByStageName(name);
                if (fi.Exists) fi.Delete();
            }
        }

        #endregion

        public Task<Errorable<Stage>> PersistStage(Stage stg)
        {
            persistStage(stg);
            return Task.FromResult((Errorable<Stage>)stg);
        }

        public Task<Errorable<Stage>> GetStageByName(StageName name)
        {
            return getStageByName(name);
        }

        public async Task<Errorable<Stage>> DeleteStageByName(StageName name)
        {
            var stg = await getStageByName(name);
            if (stg.HasErrors) return stg.Errors;

            Debug.Assert(stg.Value != null);

            deleteStage(name);

            return stg;
        }
    }
}
