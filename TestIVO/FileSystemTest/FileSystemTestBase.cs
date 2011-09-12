using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Implementation.FileSystem;
using IVO.Definition.Repositories;
using System.IO;
using System.Threading.Tasks;

namespace TestIVO.FileSystemTest
{
    public abstract class FileSystemTestBase<TtestMethods>
    {
        protected void cleanUp(FileSystem system)
        {
            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

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

        protected abstract TtestMethods getTestMethods(FileSystem system);

        protected void runTestMethod(Func<TtestMethods, Task> run)
        {
            FileSystem system = getFileSystem();

            var tm = getTestMethods(system);

            cleanUp(system);
            run(tm).Wait();
            cleanUp(system);
        }
    }
}
