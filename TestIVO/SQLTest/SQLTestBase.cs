using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.TestSupport;

namespace TestIVO.SQLTest
{
    public abstract class SQLTestBase<TtestMethods>
    {
        protected void cleanUp(DataContext db)
        {
            db.ExecuteNonQueryAsync(new ClearAllDataNonQuery()).Wait();
        }

        protected abstract TtestMethods getTestMethods(DataContext db);

        protected void runTestMethod(Func<TtestMethods, Task> run)
        {
            DataContext db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=IVO;Integrated Security=SSPI");

            var tm = getTestMethods(db);

            cleanUp(db);
            run(tm).Wait();
            cleanUp(db);
        }
    }
}
