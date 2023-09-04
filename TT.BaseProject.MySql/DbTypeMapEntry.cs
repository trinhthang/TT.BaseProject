using System.Data;

namespace TT.BaseProject.MySql
{
    public struct DbTypeMapEntry
    {
        public Type Type;
        public DbType DbType;
        public SqlDbType SqlDbType;

        public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
        {
            this.Type = type;
            this.DbType = dbType;
            this.SqlDbType = sqlDbType;
        }
    }
}