using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCMS.Data.Queries;
using GitCMS.Definition.Models;
using Asynq;
using System.Threading.Tasks;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var pr = new Program();

            pr.TestAsynq();

            Console.WriteLine("Press a key.");
            Console.ReadLine();
        }

        void TestAsynq()
        {
            var db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=GitCMS;Integrated Security=SSPI");

            var q = new CommitQuery(new CommitID(new byte[20]));

            const int max = 500;

            Task<List<Commit>>[] results = new Task<List<Commit>>[max];
            for (int i = 0; i < max; ++i)
            {
                // Asynchronously execute the query:
                results[i] = db.AsynqMulti(q);
            }

            for (int i = 0; i < max; ++i)
            {
                results[i].Wait();

                Console.WriteLine("{0} rows", results[i].Result.Count);
            }
        }
    }
}
