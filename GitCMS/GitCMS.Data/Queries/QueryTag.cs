using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public sealed class QueryTag : ISimpleDataQuery<Tag>
    {
        private Either<TagID, string> _idOrName;

        public QueryTag(TagID id)
        {
            this._idOrName = id;
        }

        public QueryTag(string name)
        {
            this._idOrName = name;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText;
            switch (_idOrName.Which)
            {
                case Either<TagID,string>.Selected.N1:
                    cmdText = String.Format(
                        @"SELECT {0} FROM {1}{2}{3} WHERE [tagid] = @tagid",
                        Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameList(),
                        Tables.TableName_Tag,
                        "", // no alias
                        Tables.TableFromHint_Tag
                    );
                    break;
                case Either<TagID,string>.Selected.N2:
                    cmdText = String.Format(
                        @"SELECT {0} FROM {1}{2}{3} WHERE [name] = @name",
                        Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameList(),
                        Tables.TableName_Tag,
                        "", // no alias
                        Tables.TableFromHint_Tag
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            if (_idOrName.Which == Either<TagID,string>.Selected.N1)
                cmd.AddInParameter("@tagid", new SqlBinary((byte[])_idOrName.N1));
            else
                cmd.AddInParameter("@name", new SqlString(_idOrName.N2));
            return cmd;
        }

        public Tag Project(SqlDataReader dr)
        {
            TagID id = (TagID) dr.GetSqlBinary(0).Value;

            Tag.Builder b = new Tag.Builder(
                pCommitID:  (CommitID)dr.GetSqlBinary(1).Value,
                pMessage:   dr.GetSqlString(2).Value
            );

            Tag tg = b;
            if (tg.ID != id) throw new InvalidOperationException();

            return tg;
        }
    }
}
