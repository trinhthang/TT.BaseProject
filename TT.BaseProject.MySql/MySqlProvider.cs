using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TT.BaseProject.MySql
{
    public class MySqlProvider : IDatabaseProvider
    {
        private static string[] _InjectionWords = new string[]{
            "\"",
            "'",
            "--",
            "#",
            "\\/*",
            "*\\/",
            "grant ",
            "drop ",
            "truncate ",
            "sleep(",
            "exec ",
            "execute ",
            "prepare ",
            "information_schema",
            "delay ",
        };

        private static string[] _InjectionIgnoreWords = new string[]{
            "''",
            "'MORE_DETAILS'",
            "'$'", "\"$\"",
            "'%'","\"%\"",
            "','","\",\"",
            "'\", \"'", "'\",\"'",
            "'\\[\"'",
            "'\"\\]'",
            "\"$[*]\"", @"\'$[*]\'",
            @"\'\$\.[a-zA-Z0-9]+\'", "\"\\$\\.[a-zA-Z0-9]+\"",
            @"'null'", "\"null\"",
            "\"ONLY_FULL_GROUP_BY,NO_UNSIGNED_SUBTRACTION,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION\"",
            "'Other'",
            "'#'",
            "Drop TEMPORARY table"
        };

        private static string _InjectionIgnoreWordsReplace = " ";
        private static Dictionary<string, string> _InjectionRemoveWords = new Dictionary<string, string>
        {
            {"\\/\\*(.*)\\*\\/", ""}
        };

        private readonly string _connectionString;
        public MySqlProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            var cnn = new MySqlConnection(_connectionString);
            return cnn;
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public IDbConnection GetOpenConnection()
        {
            var cnn = GetConnection();

            cnn.Open();

            return cnn;
        }

        public virtual void CloseConnection(IDbConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        public string ProcessSqlBeforeExecute(string sql)
        {
            var result = sql;
            //remove words
            if (_InjectionRemoveWords != null)
            {
                foreach (var item in _InjectionRemoveWords)
                {
                    result = Regex.Replace(result, item.Key, item.Value);
                }
            }

            this.ValidateSqlInjection(result);

            return result;
        }

        private void ValidateSqlInjection(string sql)
        {
            var checkSql = sql;
            //remove ignore words
            if (_InjectionIgnoreWords != null)
            {
                foreach (var item in _InjectionIgnoreWords)
                {
                    checkSql = Regex.Replace(checkSql, item, _InjectionIgnoreWordsReplace, RegexOptions.IgnoreCase);
                }
            }

            //check black words
            foreach (var item in _InjectionWords)
            {
                if (checkSql.IndexOf(item, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    throw new Exception($"Query Invalid {item} : {checkSql}");
                }
            }
        }

        private DynamicParameters BuildParams(string storeName, IDbConnection cnn, object entity, IDbTransaction transaction = null)
        {
            var dynamicParameter = new DynamicParameters();
            var entityType = entity.GetType();
            var paramArray = DeriveParameters(cnn, storeName, transaction);
            foreach (var p in paramArray)
            {
                if (p != null)
                {
                    var name = p.ParameterName;
                    name = name.Replace(name.Contains("$") ? "@$" : "@", "");
                    var pr = entityType.GetProperty(name);
                    if (pr != null)
                    {
                        object value = pr.GetValue(entity, null);
                        dynamicParameter.Add(p.ParameterName, value, this.ToDbType(pr.PropertyType), direction: p.Direction);
                    }
                }
            }
            return dynamicParameter;
        }

        protected virtual List<IDataParameter> DeriveParameters(IDbConnection cnn, string storeName, IDbTransaction transaction = null)
        {
            using (var cmd = cnn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storeName;
                cmd.Transaction = transaction;
                MySqlCommandBuilder.DeriveParameters((MySqlCommand)cmd);

                var result = new List<IDataParameter>();
                foreach (MySqlParameter item in cmd.Parameters)
                {
                    result.Add(item);
                }
                return result;
            }
        }

        private DynamicParameters GetParameters(Dictionary<string, object> param)
        {
            var result = new DynamicParameters();
            if (param != null)
            {
                foreach (var item in param)
                {
                    var value = item.Value;
                    result.Add(item.Key, value: value);
                }
            }

            return result;
        }

        public DbType ToDbType(Type type)
        {
            if (type.IsEnum)
            {
                return DbType.Int32;
            }

            DbTypeMapEntry entry = Find(type);
            return entry.DbType;
        }

        private DbTypeMapEntry Find(Type type)
        {
            foreach (var entry in _DbTypeList)
            {
                if (entry.Type == type)
                {
                    return entry;
                }
            }

            throw new ApplicationException("Reference an unsupported Type");
        }

        private static List<DbTypeMapEntry> _DbTypeList = new List<DbTypeMapEntry>()
        {
            new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit),
            new DbTypeMapEntry(typeof(bool?), DbType.Boolean, SqlDbType.Bit),
            new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt),
            new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image),
            new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Binary),
            new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime),
            new DbTypeMapEntry(typeof(DateTime?), DbType.DateTime, SqlDbType.DateTime),
            new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal),
            new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float),
            new DbTypeMapEntry(typeof(decimal?), DbType.Decimal, SqlDbType.Decimal),
            new DbTypeMapEntry(typeof(double?), DbType.Double, SqlDbType.Float),
            new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier),
            new DbTypeMapEntry(typeof(Guid?), DbType.Guid, SqlDbType.UniqueIdentifier),
            new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt),
            new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int),
            new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt),
            new DbTypeMapEntry(typeof(int?), DbType.Int32, SqlDbType.Int),
            new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant),
            new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant),
            new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar),
        };

        public int ExecuteNonQueryText(IDbConnection cnn, string commandText, object param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var result = cnn.Execute(sql, param, commandType: CommandType.Text);
            return result;
        }

        public async Task<List<T>> ExecuteQueryObjectAsync<T>(string storeName, object param = null)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteQueryObjectAsync<T>(cnn, storeName, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<List<T>> ExecuteQueryObjectAsync<T>(IDbConnection cnn, string storeName, object param = null)
        {
            var dynamicParams = BuildParams(storeName, cnn, param);
            var result = await cnn.QueryAsync<T>(storeName, dynamicParams, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<int> ExecuteNonQueryObjectAsync(string storeName, object param = null)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteNonQueryObjectAsync(cnn, storeName, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<int> ExecuteNonQueryObjectAsync(IDbConnection cnn, string storeName, object param = null)
        {
            var dynamicParams = BuildParams(storeName, cnn, param);
            var result = await cnn.ExecuteAsync(storeName, dynamicParams, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<int> ExecuteNonQueryTextAsync(string commandText, Dictionary<string, object> param, int? timeout = null)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteNonQueryTextAsync(cnn, commandText, param, timeout);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string sql)
        {
            var result = await cnn.ExecuteAsync(sql, commandType: CommandType.Text);
            return result;
        }

        public async Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param, int? timeout = null)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await cnn.ExecuteAsync(sql, dynamicParams, commandType: CommandType.Text, commandTimeout: timeout);
            return result;
        }

        public async Task<int> ExecuteNonQueryTextAsync(IDbTransaction transaction, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await transaction.Connection.ExecuteAsync(sql, dynamicParams, commandType: CommandType.Text);
            return result;
        }

        public async Task<int> ExecuteNonQueryTextAsync(string commandText, object param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteNonQueryTextAsync(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string commandText, object param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var result = await cnn.ExecuteAsync(sql, param, commandType: CommandType.Text);
            return result;
        }

        public async Task<int> ExecuteNonQueryTextAsync(IDbTransaction transaction, string commandText, object param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var result = await transaction.Connection.ExecuteAsync(sql, param, commandType: CommandType.Text);
            return result;
        }

        public async Task<object> ExecuteScalarTextAsync(string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteScalarTextAsync(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<object> ExecuteScalarTextAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await cnn.ExecuteScalarAsync(sql, dynamicParams, commandType: CommandType.Text);
            return result;
        }

        public async Task<object> ExecuteScalarTextAsync(IDbTransaction transaction, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await transaction.Connection.ExecuteScalarAsync(sql, dynamicParams, commandType: CommandType.Text);
            return result;
        }

        public async Task<object> ExecuteScalarTextAsync(string commandText, object param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.ExecuteScalarTextAsync(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<object> ExecuteScalarTextAsync(IDbConnection cnn, string commandText, object param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var result = await cnn.ExecuteAsync(sql, param, commandType: CommandType.Text);
            return result;
        }

        public async Task<object> ExecuteScalarTextAsync(IDbTransaction transaction, string commandText, object param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var result = await transaction.Connection.ExecuteAsync(sql, param, commandType: CommandType.Text);
            return result;
        }

        public async Task<List<T>> QueryAsync<T>(string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.QueryAsync<T>(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<List<T>> QueryAsync<T>(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await cnn.QueryAsync<T>(sql, dynamicParams, commandType: CommandType.Text);
            return result.AsList();
        }

        public async Task<List<T>> QueryAsync<T>(IDbTransaction transaction, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = await transaction.Connection.QueryAsync<T>(sql, dynamicParams, commandType: CommandType.Text);
            return result.AsList();
        }

        public List<T> Query<T>(string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return this.Query<T>(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public List<T> Query<T>(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var result = cnn.Query<T>(sql, dynamicParams, commandType: CommandType.Text);
            return result.AsList();
        }

        public async Task<IList> QueryAsync(Type type, string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.QueryAsync(cnn, type, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        private IList CreateList(Type type)
        {
            Type listType = typeof(List<>).MakeGenericType(new[] { type });
            IList list = (IList)Activator.CreateInstance(listType);
            return list;
        }

        public async Task<IList> QueryAsync(IDbConnection cnn, Type type, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var data = await cnn.QueryAsync(type, sql, dynamicParams, commandType: CommandType.Text) as IList;
            var result = CreateList(type);
            foreach (var item in data)
            {
                result.Add(item);
            }
            return result;
        }

        public async Task<IList> QueryAsync(IDbTransaction transaction, Type type, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var data = await transaction.Connection.QueryAsync(type, sql, dynamicParams, commandType: CommandType.Text, transaction: transaction) as IList;
            var result = CreateList(type);
            foreach (var item in data)
            {
                result.Add(item);
            }
            return result;
        }

        public async Task<IList> QueryAsync(string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = GetOpenConnection();
                return await this.QueryAsync(cnn, commandText, param);
            }
            finally
            {
                CloseConnection(cnn);
            }
        }

        public async Task<IList> QueryAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var data = await cnn.QueryAsync(sql, dynamicParams, commandType: CommandType.Text);
            return data.ToList();
        }

        public async Task<List<T>> QueryWithCommandTypeAsync<T>(IDbConnection cnn, string commandText, CommandType pCommandType, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var data = await cnn.QueryAsync<T>(sql, dynamicParams, commandType: pCommandType);
            return data.ToList();
        }

        public async Task<List<T>> QueryWithCommandTypeWithTranAsync<T>(IDbTransaction transaction, string commandText, CommandType pCommandType, Dictionary<string, object> param)
        {
            var sql = this.ProcessSqlBeforeExecute(commandText);
            var dynamicParams = this.GetParameters(param);
            var data = await transaction.Connection.QueryAsync<T>(sql, dynamicParams, commandType: pCommandType);
            return data.ToList();
        }
    }
}
