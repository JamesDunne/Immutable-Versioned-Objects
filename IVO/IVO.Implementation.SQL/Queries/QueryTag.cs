using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using IVO.Definition;
using System.Data;
using System.Threading.Tasks;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryTag : IComplexDataQuery<Errorable<Tag>>
    {
        private Either<TagID, TagName> _idOrName;

        public QueryTag(TagID id)
        {
            this._idOrName = id;
        }

        public QueryTag(TagName name)
        {
            this._idOrName = name;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText;
            switch (_idOrName.Which)
            {
                case Either<TagID, TagName>.Selected.Left:
                    cmdText = String.Format(
                        @"SELECT {0} FROM {1}{2}{3} WHERE [tagid] = @tagid",
                        Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameCommaList(),
                        Tables.TableName_Tag,
                        "", // no alias
                        Tables.TableFromHint_Tag
                    );
                    break;
                case Either<TagID, TagName>.Selected.Right:
                    cmdText = String.Format(
                        @"SELECT {0} FROM {1}{2}{3} WHERE [name] = @name",
                        Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameCommaList(),
                        Tables.TableName_Tag,
                        "", // no alias
                        Tables.TableFromHint_Tag
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            if (_idOrName.Which == Either<TagID, TagName>.Selected.Left)
                cmd.AddInParameter("@tagid", new SqlBinary((byte[])_idOrName.Left));
            else
                cmd.AddInParameter("@name", new SqlString(this._idOrName.Right.ToString()));
            return cmd;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }

        public Task<Errorable<Tag>> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return TaskEx.FromResult(Retrieve(cmd, dr, expectedCapacity));
        }

        public Errorable<Tag> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            if (!dr.Read()) return this._idOrName.Collapse<ConsistencyError>(l => new TagIDRecordDoesNotExistError(l), r => new TagNameDoesNotExistError(r));

            TagID id = (TagID)dr.GetSqlBinary(0).Value;

            Tag.Builder tb = new Tag.Builder(
                pName: (TagName)dr.GetSqlString(1).Value,
                pCommitID: (CommitID)dr.GetSqlBinary(2).Value,
                pTagger: dr.GetSqlString(3).Value,
                pDateTagged: dr.GetDateTimeOffset(4),
                pMessage: dr.GetSqlString(5).Value
            );

            Tag tg = tb;
            if (tg.ID != id) return new ComputedTagIDMismatchError(tg.ID, id);

            return tg;
        }
    }
}
