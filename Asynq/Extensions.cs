using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Threading;

namespace Asynq
{
    public static class Extensions
    {
        public static Task<SqlDataReader> ExecuteReaderAsync(this SqlCommand source, CommandBehavior behavior)
        {
            AsyncCallback callback = null;

            TaskCompletionSource<SqlDataReader> tcs = new TaskCompletionSource<SqlDataReader>(null, TaskCreationOptions.None);
            try
            {
                if (callback == null)
                {
                    callback = delegate(IAsyncResult iar)
                    {
                        Exception exception = null;
                        OperationCanceledException exception2 = null;
                        SqlDataReader result = default(SqlDataReader);
                        try
                        {
                            result = source.EndExecuteReader(iar);
                        }
                        catch (OperationCanceledException exception3)
                        {
                            exception2 = exception3;
                        }
                        catch (Exception exception4)
                        {
                            exception = exception4;
                        }
                        finally
                        {
                            if (exception2 != null)
                            {
                                tcs.TrySetCanceled();
                            }
                            else if (exception != null)
                            {
                                tcs.TrySetException(exception);
                            }
                            else
                            {
                                tcs.TrySetResult(result);
                            }
                        }
                    };
                }

                source.BeginExecuteReader(callback, null, behavior);
            }
            catch
            {
                tcs.TrySetResult(default(SqlDataReader));
                throw;
            }

            return tcs.Task;
        }
    }
}
