using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.TestSupport;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.SQLTest
{
    public abstract class SQLTestBase<TtestMethods>
    {
        protected void setSingleUser(DataContext db)
        {
            db.ExecuteNonQueryAsync(new SetSingleUserNonQuery()).Wait();
        }

        protected void cleanUp(DataContext db)
        {
            db.ExecuteNonQueryAsync(new ClearAllDataNonQuery()).Wait();
        }

        protected void setMultiUser(DataContext db)
        {
            db.ExecuteNonQueryAsync(new SetMultiUserNonQuery()).Wait();
        }

        protected abstract TtestMethods getTestMethods(DataContext db);

        protected void runTestMethod(Func<TtestMethods, Task> run)
        {
            DataContext db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=IVO;Integrated Security=SSPI");

            var tm = getTestMethods(db);

            // Set single-user mode:
            try
            {
                setSingleUser(db);
            }
            catch (SqlException)
            {
                Thread.Sleep(10);
                try
                {
                    setSingleUser(db);
                }
                catch (SqlException sqex)
                {
                    Assert.Fail(sqex.ToString());
                }
            }
            Thread.Sleep(10);

            // Delete all data:
            try
            {
                cleanUp(db);
            }
            catch (SqlException)
            {
                // Try again?
                Thread.Sleep(10);
                try
                {
                    cleanUp(db);
                }
                catch (SqlException sqex)
                {
                    Assert.Fail(sqex.ToString());
                }
            }
            // Wait after cleanup for the SQL server to reset its state:
            Thread.Sleep(10);

            // Set multi-user mode:
            try
            {
                setMultiUser(db);
            }
            catch (SqlException)
            {
                // Try again?
                Thread.Sleep(10);
                try
                {
                    setMultiUser(db);
                }
                catch (SqlException sqex)
                {
                    Assert.Fail(sqex.ToString());
                }
            }

            // Run the test method now:
            run(tm).Wait();
        }
    }
}
