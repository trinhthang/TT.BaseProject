using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Cache;
using TT.BaseProject.Cache.Constants;
using TT.BaseProject.Cache.Models;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Base;
using TT.BaseProject.Domain.Constant;
using TT.BaseProject.Domain.Crud;
using TT.BaseProject.Domain.Filter;
using TT.BaseProject.MySql;

namespace TT.BaseProject.Domain.MySql
{
    public class MySqlRepo : IBaseRepo
    {
        #region Declaration
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ITypeService _typeService;
        protected readonly ISerializerService _serializerService;
        protected readonly ICacheService _cacheService;

        protected readonly string _connectionString;
        protected IDatabaseProvider _databaseProvider;

        protected const int MAX_QUERY_LENGTH = 60000;
        protected static readonly List<Type> AUTO_ID_TYPES = new List<Type> { typeof(int), typeof(long) };
        #endregion

        #region Constructor
        public MySqlRepo(string connection, IServiceProvider serviceProvider)
        {
            _connectionString = connection;
            _serviceProvider = serviceProvider;
            _typeService = serviceProvider.GetRequiredService<ITypeService>();
            _serializerService = serviceProvider.GetRequiredService<ISerializerService>();
            _cacheService = serviceProvider.GetRequiredService<ICacheService>();
        }
        #endregion

        #region Properties
        protected IDatabaseProvider Provider
        {
            get
            {
                if (_databaseProvider == null)
                {
                    _databaseProvider = this.CreateProvider(_connectionString);
                }

                return _databaseProvider;
            }
        }
        #endregion

        /// <summary>
        /// Khởi tạo provider thao tác với database
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        protected virtual IDatabaseProvider CreateProvider(string connectionString)
        {
            return new MySqlProvider(connectionString);
        }

        #region Implement

        public IDbConnection GetOpenConnection()
        {
            var cnn = this.Provider.GetConnection();
            cnn.Open();
            return cnn;
        }

        public IDbConnection GetConnection()
        {
            var cnn = this.Provider.GetConnection();
            return cnn;
        }

        public void CloseConnection(IDbConnection cnn)
        {
            this.Provider.CloseConnection(cnn);
        }

        public async Task<bool> DeleteAsync(object entity)
        {
            var query = this.GetDeleteQuery(entity.GetType());
            var res = await this.Provider.ExecuteNonQueryTextAsync(query, entity);
            return res > 0;
        }

        public async Task<bool> DeleteAsync(IDbTransaction transaction, object entity)
        {
            var query = this.GetDeleteQuery(entity.GetType());
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, entity);
            return res > 0;
        }

        public async Task<bool> DeleteAsync(IDbTransaction transaction, Type type, object entity)
        {
            var query = this.GetDeleteQuery(type);
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, entity);
            return res > 0;
        }

        public async Task<bool> DeleteAsync(IDbConnection cnn, object entity)
        {
            var query = this.GetDeleteQuery(entity.GetType());
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, entity);
            return res > 0;
        }

        public async Task<bool> DeletesAsync(IDbConnection cnn, Type type, IList ids)
        {
            var key = _typeService.GetKeyField(type);
            return await this.DeletesAsync(cnn, type, key.Name, ids);
        }

        public async Task<bool> DeletesAsync(IDbConnection cnn, Type type, string field, IList values)
        {
            var param = new Dictionary<string, object>();
            string tableName = null;
            if (type != null)
            {
                var tableAttr = type.GetCustomAttribute<TableAttribute>();
                tableName = tableAttr.Table;
            }

            var query = this.GetDeleteQuery(tableName, field, values, param);
            var result = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, param);
            return result > 0;
        }

        public async Task<bool> DeletesAsync(IDbTransaction transaction, Type type, string field, IList values)
        {
            var param = new Dictionary<string, object>();
            string tableName = null;
            if (type != null)
            {
                var tableAttr = type.GetCustomAttribute<TableAttribute>();
                tableName = tableAttr.Table;
            }

            var query = this.GetDeleteQuery(tableName, field, values, param);
            var result = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, param);
            return result > 0;
        }

        protected virtual string GetDeleteQuery(Type type)
        {
            var cacheParam = new CacheParam()
            {
                Name = CacheItemName.SqlDelete,
                Custom = type.FullName
            };

            var query = _cacheService.Get<string>(cacheParam);

            if (string.IsNullOrEmpty(query))
            {
                var key = _typeService.GetKeyField(type);
                var table = this.GetTableName(type);
                query = $"DELETE FROM {table} WHERE {key.Name} = @{key.Name}";
                _cacheService.Set(cacheParam, query);
            }

            return query;
        }

        protected virtual string GetDeleteQuery(string table, string field, IList values, Dictionary<string, object> param)
        {
            var sb = new StringBuilder($"DELETE FROM {this.SafeTable(table)} WHERE {this.SafeColumn(field)} IN (");
            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                var p = $"p{i}";
                param[p] = values[i];
                sb.Append($"@{p}");
            }
            sb.Append(")");
            return sb.ToString();
        }

        protected virtual string GetTableName(Type type)
        {
            return this.SafeTable(_typeService.GetTableName(type));
        }

        protected virtual string GetTableName<T>()
        {
            return this.GetTableName(typeof(T));
        }

        protected virtual string BuildSelectByFieldQuery(Type type, Dictionary<string, object> param, string field, object value, string op = "=", string columns = "*")
        {
            var sop = this.SafeOperator(op);
            var table = this.GetTableName(type);
            var sb = new StringBuilder($"SELECT {columns} FROM {table} WHERE {field} {sop} ");
            if (sop == FilterOperator.In || sop == FilterOperator.NotIn)
            {
                IList vl = (IList)value;
                sb.Append("(");
                for (int i = 0; i < vl.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    var p = $"p{i}";
                    sb.Append($"@{p}");
                    param[p] = vl[i];
                }
                sb.Append(")");
            }
            else
            {
                sb.Append("@value");
                param["value"] = value;
            }

            return sb.ToString();
        }

        public async Task<List<T>> GetAsync<T>(string field, object value, string op = "=")
        {
            var sop = this.SafeOperator(op);
            var param = new Dictionary<string, object>();
            var sql = this.BuildSelectByFieldQuery(typeof(T), param, field, value, op = sop);
            var result = await this.Provider.QueryAsync<T>(sql, param);

            return result;
        }

        public async Task<List<T>> GetAsync<T>(IDbConnection cnn, string field, object value, string op = "=", string columns = "*")
        {
            var sop = this.SafeOperator(op);
            var param = new Dictionary<string, object>();
            var sql = this.BuildSelectByFieldQuery(typeof(T), param, field, value, op = sop);
            var result = await this.Provider.QueryAsync<T>(cnn, sql, param);

            return result;
        }

        public async Task<List<T>> GetAsync<T>(IDbTransaction transaction, string field, object value, string op = "=")
        {
            var sop = this.SafeOperator(op);
            var param = new Dictionary<string, object>();
            var sql = this.BuildSelectByFieldQuery(typeof(T), param, field, value, op = sop);
            var result = await this.Provider.QueryAsync<T>(transaction, sql, param);

            return result;
        }

        public async Task<List<T>> GetAsync<T>()
        {
            var table = this.GetTableName(typeof(T));
            var query = $"SELECT * FROM {table}";
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync<T>(query, param);

            return result;
        }

        public async Task<List<T>> GetAsync<T>(IDbConnection cnn)
        {
            var table = this.GetTableName(typeof(T));
            var query = $"SELECT * FROM {table}";
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync<T>(cnn, query, param);
            return result;
        }

        public async Task<List<T>> GetAsync<T>(List<FilterItem> filters, string sort = null, int skip = 0, int? take = null, string emptyFilter = null)
        {
            var sortSql = this.ParseSort(sort);
            var param = new Dictionary<string, object>();
            var where = this.ParseWhere(filters, param);
            var takeSelect = this.ProcessTake(take ?? 0);
            var table = this.GetTableName<T>();

            IDbConnection cnn = null;
            List<T> result = null;
            try
            {
                cnn = this.Provider.GetOpenConnection();

                var sb = new StringBuilder($"SELECT * FROM {table}");
                if (!string.IsNullOrWhiteSpace(where))
                {
                    sb.Append($" WHERE {where}");
                }

                if (!string.IsNullOrWhiteSpace(sortSql))
                {
                    sb.Append($" ORDER BY {sortSql}");
                }

                if (take != null)
                {
                    sb.Append($" LIMIT {takeSelect}");
                }

                if (skip > 0)
                {
                    sb.Append($" WHERE {skip}");
                }

                result = await this.Provider.QueryAsync<T>(cnn, sb.ToString(), param);
            }
            finally
            {
                this.Provider.CloseConnection(cnn);
            }

            return result;
        }

        protected virtual string BuildQueryById(Type type)
        {
            var table = this.GetTableName(type);
            var prKey = _typeService.GetKeyField(type);
            return $"SELECT * FROM {table} WHERE {prKey.Name} = @key";
        }

        protected virtual string BuildQueryByIds(IList ids, Type type, Dictionary<string, object> param)
        {
            var table = this.GetTableName(type);
            var key = _typeService.GetKeyField(type);
            var sb = new StringBuilder($"SELECT * FROM {table} WHERE {key.Name} IN (");
            var count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                var p = $"p{i}";
                sb.Append($"@{p}");
                param[p] = ids[i];
            }
            sb.Append(")");
            return sb.ToString();
        }

        public T GetById<T>(object id)
        {
            var sql = this.BuildQueryById(typeof(T));
            var result = this.Provider.Query<T>(sql, new Dictionary<string, object> { { "key", id } });
            return result.FirstOrDefault();
        }

        public async Task<T> GetByIdAsync<T>(object id)
        {
            var sql = this.BuildQueryById(typeof(T));
            var result = await this.Provider.QueryAsync<T>(sql, new Dictionary<string, object> { { "key", id } });
            return result.FirstOrDefault();
        }

        public async Task<T> GetByIdAsync<T>(IDbConnection cnn, object id)
        {
            var sql = this.BuildQueryById(typeof(T));
            var result = await this.Provider.QueryAsync<T>(cnn, sql, new Dictionary<string, object> { { "key", id } });
            return result.FirstOrDefault();
        }

        public async Task<T> GetByIdAsync<T>(IDbConnection cnn, Type type, object id)
        {
            var sql = this.BuildQueryById(type);
            var result = await this.Provider.QueryAsync<T>(cnn, sql, new Dictionary<string, object> { { "key", id } });
            return result.FirstOrDefault();
        }

        public List<T> GetByIds<T>(IList ids)
        {
            var param = new Dictionary<string, object>();
            var sql = this.BuildQueryByIds(ids, typeof(T), param);
            var result = this.Provider.Query<T>(sql, param);
            return result;
        }

        public async Task<List<T>> GetByIdsAsync<T>(IList ids)
        {
            var param = new Dictionary<string, object>();
            var sql = this.BuildQueryByIds(ids, typeof(T), param);
            var result = await this.Provider.QueryAsync<T>(sql, param);
            return result;
        }

        public async Task<IList> GetComboboxPagingAsync(Type type, string sort, int skip, int take, string columns, string filter, string selectedItem)
        {
            var columnSql = this.ParseColumn(columns);
            var sortSql = this.ParseSort(sort);
            var param = new Dictionary<string, object>();
            var where = this.ParseWhere(filter, param);
            var takeSelect = this.ProcessTake(take);
            var table = this.GetTableName(type);

            IDbConnection cnn = null;
            IList result = null;
            try
            {
                cnn = this.Provider.GetOpenConnection();
                var sb = new StringBuilder($"SELECT {columnSql} FROM {table}");
                if (!string.IsNullOrWhiteSpace(where))
                {
                    sb.Append($" WHERE {where}");
                }
                if (!string.IsNullOrEmpty(sortSql))
                {
                    sb.Append($" ORDER BY {sortSql}");
                }
                sb.Append($" LIMIT {takeSelect}");
                if (skip > 0)
                {
                    sb.Append($" OFFSET {skip}");
                }

                result = await this.Provider.QueryAsync(cnn, sb.ToString(), param);

                if (skip == 0 && !string.IsNullOrEmpty(selectedItem))
                {
                    var selectedParam = _serializerService.DeserializeObject<Dictionary<string, object>>(selectedItem);
                    var selectedObject = selectedParam.FirstOrDefault();
                    var value = selectedObject.Value;

                    var existIndex = -1;
                    if (result.Count > 0)
                    {
                        if (value is JArray jArrayValue)
                        {
                            var arrValue = jArrayValue.Select(x =>
                            {
                                return x.ToString();
                            }).ToList();

                            var index = 0;
                            foreach (IDictionary<string, object> item in result)
                            {
                                var typeCompare = type.GetProperty(selectedObject.Key).PropertyType;
                                if (typeCompare != null)
                                {
                                    if (arrValue.Any(x => x == item[selectedObject.Key].ToString()))
                                    {
                                        arrValue.Remove(item[selectedObject.Key].ToString());
                                    }
                                }

                                if (arrValue.Count == 0)
                                {
                                    existIndex = index;
                                    break;
                                }
                                index++;
                            }
                            selectedParam[selectedObject.Key] = arrValue;
                        }
                        else
                        {
                            var index = 0;
                            foreach (IDictionary<string, object> item in result)
                            {
                                var match = true;

                                var typeCompare = type.GetProperty(selectedObject.Key).PropertyType;
                                if (typeCompare != null)
                                {
                                    if (item[selectedObject.Key].ToString() != selectedObject.Value.ToString())
                                    {
                                        match = false;
                                    }
                                }

                                if (match)
                                {
                                    existIndex = index;
                                    break;
                                }
                                index++;
                            }
                        }

                        if (existIndex == -1)
                        {
                            var op = value is JArray ? "IN" : "=";
                            sb.Clear();
                            sb.Append($"SELECT {columnSql} FROM {table} WHERE");
                            var keys = selectedParam.Select(x => x.Key).ToList();
                            for (int i = 0; i < keys.Count; i++)
                            {
                                if (i > 0)
                                {
                                    sb.Append(" AND");
                                }
                                sb.AppendFormat(" `{0}` {1} @{0}", keys[i], op);
                            }

                            if (!string.IsNullOrEmpty(sortSql))
                            {
                                sb.Append($" ORDER BY {sortSql}");
                            }
                            if (op == "=")
                            {
                                sb.Append(" LIMIT 1;");
                            }
                            var selectedRecord = (await this.Provider.QueryAsync(cnn, sb.ToString(), selectedParam));
                            if (selectedRecord != null && selectedRecord.Count > 0)
                            {
                                var i = 0;
                                foreach (var item in selectedRecord)
                                {
                                    result.Insert(i, item);
                                    i++;
                                }

                            }
                            if (value is JArray jArray)
                            {
                                HandleSelectedForMultipleCombobox(jArray, result, selectedObject);
                            }
                        }
                        else
                        {
                            var temp = result[existIndex];
                            result.Remove(temp);
                            result.Insert(0, temp);
                        }
                    }
                }
            }
            finally
            {
                this.Provider.CloseConnection(cnn);
            }

            return result;
        }

        protected virtual void HandleSelectedForMultipleCombobox(JArray jArray, IList result, KeyValuePair<string, object> selectedObject)
        {
            var orderData = result.Cast<IDictionary<string, object>>().OrderBy(x => x[selectedObject.Key]).ToList();

            var listSelected = orderData.Where(x => jArray.Any(y => x[selectedObject.Key].Equals(y.ToString())));
            var listQueryMore = orderData.Where(x => !jArray.Any(y => x[selectedObject.Key].Equals(y.ToString())));
            result = listSelected.Concat(listQueryMore).ToList();
        }

        public List<TEntity> GetDynamic<TEntity>(string columns, string field, object value, string op = "=")
        {
            var type = typeof(TEntity);
            var sop = this.SafeOperator(op);
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table} WHERE {field} {sop} @value;";
            var param = new Dictionary<string, object>() { { "value", value } };
            var result = this.Provider.Query<TEntity>(query, param);
            return result;
        }

        public async Task<IList> GetDynamicAsync(Type type, string columns)
        {
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table}";
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync(query, param);
            return result;
        }

        public async Task<IList> GetDynamicAsync(Type type, string columns, string sorts)
        {
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var sort = this.ParseSort(sorts);
            var query = $"SELECT {column} FROM {table}";
            if (!string.IsNullOrEmpty(sort))
            {
                query = $"{query} ORDER BY {sort}";
            }
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync(type, query, param);
            return result;
        }

        public async Task<IList> GetDynamicAsync(IDbConnection cnn, Type type, string columns)
        {
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table}";
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync(cnn, type, query, param);
            return result;
        }

        public async Task<IList> GetDynamicAsync(Type type, string columns, string field, object value, string op = "=")
        {
            var sop = this.SafeOperator(op);
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table} WHERE {field} {sop} @value;";
            var param = new Dictionary<string, object>() { { "value", value } };
            var result = await this.Provider.QueryAsync(query, param);
            return result;
        }

        public async Task<List<T>> GetDynamicAsync<T>(Type type, string columns, string field, object value, string op = "=")
        {
            var sop = this.SafeOperator(op);
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table} WHERE {field} {sop} @value;";
            var param = new Dictionary<string, object>() { { "value", value } };
            var result = await this.Provider.QueryAsync<T>(query, param);
            return result;
        }

        public async Task<List<T>> GetDynamicAsync<T>(string columns)
        {
            var type = typeof(T);
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table}";
            Dictionary<string, object> param = null;
            var result = await this.Provider.QueryAsync<T>(query, param);
            return result;
        }

        public async Task<List<T>> GetDynamicAsync<T>(string columns, string field, object value, string op = "=")
        {
            var type = typeof(T);
            var sop = this.SafeOperator(op);
            var table = this.GetTableName(type);
            var column = this.ParseDynamicColumn(columns);
            var query = $"SELECT {column} FROM {table} WHERE {field} {sop} @value;";
            var param = new Dictionary<string, object>() { { "value", value } };
            var result = await this.Provider.QueryAsync<T>(query, param);
            return result;
        }

        public async Task<PagingResult> GetPagingAsync(Type type, string sort, int skip, int take, string columns, string filter = null, string emptyFilter = null)
        {
            var columnSql = this.ParseColumn(columns, "t");
            var sortSql = this.ParseSort(sort);
            var param = new Dictionary<string, object>();
            var where = this.ParseWhere(filter, param, "t");
            var takeSelect = this.ProcessTake(take);
            var table = this.GetTableName(type);

            IDbConnection cnn = null;
            var result = new PagingResult();
            try
            {
                cnn = this.Provider.GetOpenConnection();
                var sb = new StringBuilder($"SELECT {columnSql} FROM {table} t ");
                if (!string.IsNullOrWhiteSpace(where))
                {
                    sb.Append($" WHERE {where} ");
                }
                if (!string.IsNullOrWhiteSpace(sortSql))
                {
                    sb.Append($" ORDER BY {sortSql}");
                }
                sb.Append($" LIMIT {takeSelect}");
                if (skip > 0)
                {
                    sb.Append($" OFFSET {skip}");
                }

                result.PageData = await this.Provider.QueryAsync(cnn, sb.ToString(), param);

                if (skip == 0 && result.PageData.Count == 0 && emptyFilter != null)
                {
                    result.Empty = await this.CheckSourceEmptyAsync(cnn, table, emptyFilter);
                }
            }
            finally
            {
                this.CloseConnection(cnn);
            }

            return result;
        }

        protected virtual async Task<bool> CheckSourceEmptyAsync(IDbConnection cnn, string table, string filter)
        {
            var param = new Dictionary<string, object>();
            var where = this.ParseWhere(filter, param);
            var sb = new StringBuilder($"SELECT COUNT(*) FROM {table}");
            if (!string.IsNullOrEmpty(where))
            {
                sb.Append($" WHERE {where}");
            }
            var exist = Convert.ToInt32(await this.Provider.ExecuteScalarTextAsync(cnn, sb.ToString(), param));
            return exist == 0;
        }

        public async Task<PagingSummaryResult> GetPagingSummaryAsync(Type type, string columns, string filter = null)
        {
            var columnSql = this.ParseColumn(columns, "t");
            var param = new Dictionary<string, object>();
            var where = this.ParseWhere(filter, param, "t");
            var table = this.GetTableName(type);

            IDbConnection cnn = null;
            var result = new PagingSummaryResult();
            try
            {
                cnn = this.Provider.GetOpenConnection();
                var sb = new StringBuilder($"SELECT COUNT(*) AS _Total");

                if (!string.IsNullOrWhiteSpace(columnSql))
                {
                    sb.Append($",{columnSql}");
                }
                sb.Append($" FROM {table} t ");
                if (!string.IsNullOrWhiteSpace(where))
                {
                    sb.Append($" WHERE {where} ");
                }
                var sumResult = await this.Provider.QueryAsync(cnn, sb.ToString(), param);
                var sum = ((IList)sumResult)[0] as IDictionary<string, object>;
                result.SummaryData = sum;
                result.Total = Convert.ToInt32(sum["_Total"]);
            }
            finally
            {
                this.CloseConnection(cnn);
            }

            return result;
        }

        public async Task<IList> GetTreeAsync<T>(string columns, string sort)
        {
            var columnSql = this.ParseColumn(columns);
            var sortSql = this.ParseSort(sort);
            var tableName = this.GetTableName(typeof(T));
            var sb = new StringBuilder($"SELECT {columnSql} FROM {tableName}");
            if (!string.IsNullOrEmpty(sortSql))
            {
                sb.Append($" ORDER BY {sortSql}");
            }
            var data = await this.Provider.QueryAsync<T>(sb.ToString(), null);
            return data;
        }

        protected virtual string GetInsertQuery(Type type, object entity)
        {
            var cacheParam = new CacheParam()
            {
                Name = CacheItemName.SqlInsert,
                Custom = type.FullName
            };

            var query = _cacheService.Get<string>(cacheParam);
            if (string.IsNullOrEmpty(query))
            {

                var fields = _typeService.GetTableColumns(type);
                var tableName = this.GetTableName(type);
                query = $"INSERT INTO {tableName} (`{string.Join("`,`", fields)}`) VALUES(@{string.Join(",@", fields)});";
                var keys = _typeService.GetKeyFields(type);

                if (keys.Count == 1 && AUTO_ID_TYPES.Contains(keys[0].PropertyType))
                {
                    query += "select last_insert_id();";
                }
                _cacheService.Set(cacheParam, query);
            }
            return query;
        }

        protected virtual void updateEntityKey(object entity, object executeResult)
        {
            if (executeResult != null)
            {
                var pkId = _typeService.GetKeyField(entity.GetType());
                if (pkId != null)
                {
                    if (pkId.PropertyType == typeof(Int32))
                    {
                        pkId.SetValue(entity, Convert.ToInt32(executeResult));
                    }
                    else if (pkId.PropertyType == typeof(Int64))
                    {
                        pkId.SetValue(entity, Convert.ToInt64(executeResult));
                    }
                }
            }
        }

        public async Task<object> InsertAsync(object entity)
        {
            var query = this.GetInsertQuery(entity.GetType(), entity);
            var res = await this.Provider.ExecuteScalarTextAsync(query, entity);
            if ((res is int && (int)res > 0) || (res is long && (long)res > 0))
            {
                this.updateEntityKey(entity, res);
            }

            return res;
        }

        public async Task<object> InsertAsync(IDbConnection cnn, object entity)
        {
            var query = this.GetInsertQuery(entity.GetType(), entity);
            var res = await this.Provider.ExecuteScalarTextAsync(cnn, query, entity);
            if ((res is int && (int)res > 0) || (res is long && (long)res > 0))
            {
                this.updateEntityKey(entity, res);
            }

            return res;
        }

        public async Task<object> InsertAsync(IDbTransaction transaction, object entity)
        {
            return await this.InsertAsync(transaction, entity.GetType(), entity);
        }

        public async Task<object> InsertAsync(IDbTransaction transaction, Type type, object entity)
        {
            var query = this.GetInsertQuery(type, entity);
            var res = await this.Provider.ExecuteScalarTextAsync(transaction, query, entity);

            if ((res is int && (int)res > 0)
                || (res is uint && (uint)res > 0)
                || (res is long && (long)res > 0)
                || (res is ulong && (ulong)res > 0))
            {
                this.updateEntityKey(entity, res);
            }

            return res;
        }

        public async Task InsertsAsync(IList entities)
        {
            using var cnn = this.GetOpenConnection();
            var query = this.GetInsertQuery(_typeService.GetTypeInList(entities.GetType()), entities[0]);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, entities);
        }

        public async Task InsertsAsync(IList entities, IDbConnection cnn)
        {
            var query = this.GetInsertQuery(_typeService.GetTypeInList(entities.GetType()), entities[0]);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, entities);
        }

        public async Task InsertsAsync(IList entities, IDbTransaction transaction)
        {
            var query = this.GetInsertQuery(_typeService.GetTypeInList(entities.GetType()), entities[0]);
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, entities);
        }

        public async Task InsertBatchAsync(IDbConnection cnn, IList entities)
        {
            throw new NotImplementedException();
        }

        public async Task InsertBatchAsync(IDbConnection cnn, IList entities, Type type)
        {
            throw new NotImplementedException();
        }

        public async Task InsertBatchAsync(IDbTransaction transaction, IList entities)
        {
            throw new NotImplementedException();
        }

        public async Task InsertBatchAsync(IDbTransaction transaction, IList entities, Type type)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetUpdateQuery(Type type, object entity, string fields = null)
        {
            if (fields == null)
            {
                fields = string.Empty;
            }

            var cacheParam = new CacheParam()
            {
                Name = CacheItemName.SqlUpdate,
                Custom = $"{type.FullName}_{fields}"
            };

            var query = _cacheService.Get<string>(cacheParam);
            if (string.IsNullOrEmpty(query))
            {


                var columns = _typeService.GetTableColumns(type);
                var key = _typeService.GetKeyField(type);
                List<string> updateFields;
                if (string.IsNullOrEmpty(fields))
                {
                    updateFields = columns.Where(n => n != key.Name).ToList();
                }
                else
                {
                    updateFields = new List<string>();
                    foreach (var column in fields.Split(","))
                    {
                        foreach (var field in columns)
                        {
                            if (field.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {
                                updateFields.Add(field);
                            }
                        }
                    }
                }

                var table = this.GetTableName(type);
                if (string.IsNullOrEmpty(table)) throw new NullReferenceException($"Not found table in type {type}");

                query = $"UPDATE {table} SET {string.Join(", ", updateFields.Select(n => $"`{n}`=@{n}"))} WHERE `{key.Name}`=@{key.Name};";

                _cacheService.Set(cacheParam, query);
            }
            return query;
        }

        public bool Update(IDbConnection cnn, object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(entity.GetType(), entity, fields);
            var res = this.Provider.ExecuteNonQueryText(cnn, query, entity);
            return res > 0;
        }

        public async Task<bool> UpdateAsync(object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(entity.GetType(), entity, fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(query, entity);
            return res > 0;
        }

        public async Task<bool> UpdateAsync(IDbTransaction transaction, object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(entity.GetType(), entity, fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, entity);
            return res > 0;
        }

        public async Task<bool> UpdateAsync(IDbTransaction transaction, Type type, object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(type, entity, fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, query, entity);
            return res > 0;
        }

        public async Task<bool> UpdateAsync(IDbConnection cnn, object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(entity.GetType(), entity, fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, entity);
            return res > 0;
        }

        public async Task<bool> UpdateAsync(IDbConnection cnn, Type type, object entity, string fields = null)
        {
            var query = this.GetUpdateQuery(type, entity, fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, query, entity);
            return res > 0;
        }

        public async Task<int> UpdatesAsync(IList entities, Type typeEntity = null, string fields = null)
        {
            using var cnn = this.GetOpenConnection();
            if (entities.Count == 0) return 0;
            var type = typeEntity == null ? entities[0].GetType() : typeEntity;
            var updateQuery = this.GetUpdateQuery(type, entities[0], fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, updateQuery, entities);

            return res;
        }

        public async Task<int> UpdatesAsync(IDbConnection cnn, IList entities, Type typeEntity = null, string fields = null)
        {
            if (entities.Count == 0) return 0;
            var type = typeEntity == null ? entities[0].GetType() : typeEntity;
            var updateQuery = this.GetUpdateQuery(type, entities[0], fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(cnn, updateQuery, entities);

            return res;
        }

        public async Task<int> UpdatesAsync(IDbTransaction transaction, IList entities, Type typeEntity = null, string fields = null)
        {
            if (entities.Count == 0) return 0;
            var type = typeEntity == null ? entities[0].GetType() : typeEntity;
            var updateQuery = this.GetUpdateQuery(type, entities[0], fields);
            var res = await this.Provider.ExecuteNonQueryTextAsync(transaction, updateQuery, entities);

            return res;
        }

        public bool UpdateMulti(IDbConnection cnn, Type type, string fields, object value, IList ids)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateMultiAsync(IDbConnection cnn, Type type, string fields, object value, IList ids)
        {
            throw new NotImplementedException();
        }

        protected virtual string ParseSort(string sorts)
        {
            if (string.IsNullOrWhiteSpace(sorts))
            {
                return "";
            }

            var sb = new StringBuilder();
            foreach (var sort in sorts.Split(","))
            {
                if (string.IsNullOrWhiteSpace(sort))
                {
                    continue;
                }

                var item = sort.Trim();
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                var ix = item.LastIndexOf(" ");
                string field;
                var dir = "ASC";
                if (ix > 0)
                {
                    field = item.Substring(0, ix);
                    var temp = item.Substring(ix + 1);
                    if ("DESC".Equals(temp, StringComparison.OrdinalIgnoreCase))
                    {
                        dir = "DESC";
                    }
                }
                else
                {
                    field = item;
                }

                field = field.Trim();
                if (string.IsNullOrEmpty(field))
                {
                    continue;
                }

                sb.Append($"`{field}` {dir}");
            }

            return sb.ToString();
        }

        protected virtual string ParseWhere(string filter, Dictionary<string, object> param, string alias = "")
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return "";
            }

            var items = _serializerService.DeserializeObject<List<FilterItem>>(filter);
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var sql = this.ParseFilter(item, param, string.IsNullOrEmpty(item.Alias) ? alias : item.Alias);
                if (string.IsNullOrEmpty(sql))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(sql);
            }

            return sb.ToString();
        }

        protected virtual string ParseWhere(List<FilterItem> filters, Dictionary<string, object> param, string alias = "")
        {
            if (filters.Any())
            {
                return "";
            }

            var sb = new StringBuilder();
            foreach (var item in filters)
            {
                var sql = this.ParseFilter(item, param, alias);
                if (string.IsNullOrEmpty(sql))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(sql);
            }

            return sb.ToString();
        }

        protected string ParseFilter(FilterItem filter, Dictionary<string, object> param, string alias = "")
        {
            var sb = new StringBuilder();
            var hasOr = filter.Ors != null && filter.Ors.Any();
            var op = string.IsNullOrEmpty(filter.Operator) ? "=" : filter.Operator.ToUpper();

            if (hasOr || op.Equals("NULL"))
            {
                sb.Append("(");
            }

            sb.Append(SafeColumn(filter.Field, alias));

            var pname = $"{filter.Field}{param.Count}";
            switch (op)
            {
                case FilterOperator.Equals:
                case FilterOperator.GreaterThan:
                case FilterOperator.GreaterThanEquals:
                case FilterOperator.LessThan:
                case FilterOperator.LessThanEquals:
                case FilterOperator.NotEquals:
                    sb.Append($" {op} @{pname}");
                    param[pname] = this.GetFilterValue(filter.Field, filter.Value);
                    break;
                case FilterOperator.Contains:
                    sb.Append($" LIKE @{pname}");
                    param[pname] = $"%{this.GetFilterValue(filter.Field, filter.Value)}%";
                    break;
                case FilterOperator.EndsWith:
                    sb.Append($" LIKE @{pname}");
                    param[pname] = $"%{this.GetFilterValue(filter.Field, filter.Value)}";
                    break;
                case FilterOperator.StartsWith:
                    sb.Append($" LIKE @{pname}");
                    param[pname] = $"{this.GetFilterValue(filter.Field, filter.Value)}%";
                    break;
                case FilterOperator.Notcontains:
                    sb.Append($" NOT LIKE @{pname}");
                    param[pname] = $"%{this.GetFilterValue(filter.Field, filter.Value)}%";
                    break;
                case FilterOperator.Null:
                    if (filter.Value != null && filter.Value.ToString() == "1000-01-01")
                    {
                        sb.Append($" IS NULL ");
                    }
                    else
                    {
                        sb.Append($" IS NULL OR {SafeColumn(filter.Field, alias)} = ''");
                    }
                    break;
                case FilterOperator.NotNull:
                    if (filter.Value != null && filter.Value.ToString() == "1000-01-01")
                    {
                        sb.Append($" IS NOT NULL ");
                    }
                    else
                    {
                        sb.Append($" IS NOT NULL OR {SafeColumn(filter.Field, alias)} = ''");
                    }
                    break;
                case FilterOperator.In:
                case FilterOperator.NotIn:
                    if (filter.Value is IList)
                    {
                        sb.Append($" {op} (");
                        var values = (IList)filter.Value;
                        for (int i = 0; i < values.Count; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(",");
                            }

                            object item = values[i];
                            if (item is JValue)
                            {
                                item = ((JValue)item).Value;
                            }
                            pname = $"{filter.Field}{param.Count}_{i}";
                            sb.Append($"@{pname}");

                            var value = this.GetFilterValue(filter.Field, item);
                            param[pname] = this.GetFilterValue(filter.Field, value);
                        }
                        sb.Append(";");
                    }
                    else
                    {
                        return null;
                    }
                    break;
                default:
                    return null;
                    break;
            }

            if (hasOr || op.Equals("NULL"))
            {
                if (hasOr)
                {
                    foreach (var item in filter.Ors)
                    {
                        var temp = this.ParseFilter(item, param, string.IsNullOrEmpty(item.Alias) ? alias : item.Alias);
                        sb.Append($" OR {temp}");
                    }
                }
                sb.Append(")");
            }

            return sb.ToString();
        }

        protected virtual string ParseColumn(string columns, string alias = "")
        {
            if (string.IsNullOrWhiteSpace(columns))
            {
                throw new ArgumentException("Invalid column");
            }

            var res = new List<string>();
            var sb = new StringBuilder();
            foreach (var item in columns.Split(","))
            {
                if (this.CheckIgnoreColumn(item))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                res.Add(SafeColumn(item, alias));
            }

            return string.Join(",", res);
        }

        protected virtual string ParseDynamicColumn(string columns)
        {
            if (columns == "*")
            {
                return columns;
            }

            return this.ParseColumn(columns);
        }

        protected bool CheckIgnoreColumn(string column)
        {
            if (string.IsNullOrWhiteSpace(column)
                || column.Contains("'")
                || column.Contains("`")
                || column.Contains("(")
                || column.Contains(")"))
            {
                return true;
            }

            return false;
        }

        protected virtual string SafeColumn(string column, string alias = "")
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(alias))
            {
                sb.Append($"{alias}.`");
            }
            else
            {
                sb.Append("`");
            }
            Char ch;
            for (int i = 0; i < column.Length; i++)
            {
                ch = column[i];
                switch (ch)
                {
                    case ' ':
                    case '`':
                    case '\\':
                        continue;
                }
                sb.Append(ch);
            }
            sb.Append("`");
            return sb.ToString();
        }

        protected virtual string SafeTable(string table)
        {
            return $"`{table}`";
        }

        protected string SafeOperator(string op)
        {
            if (op.Contains("'") || op.Contains(";"))
            {
                throw new NotImplementedException($"Not support operator {op}");
            }
            return op;
        }

        protected object GetFilterValue(string field, object value)
        {
            if (value is string)
            {
                DateTime tempDate;
                if (field.Contains("Time") && DateTime.TryParseExact(value as string, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                {
                    return tempDate;
                }
                else if (field.Contains("Date") && DateTime.TryParseExact(value as string, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                {
                    return tempDate;
                }
            }

            return value;
        }

        protected virtual int ProcessTake(int take)
        {
            if (take <= 0 || take > 5000)
            {
                return 20;
            }

            return take;
        }
        #endregion
    }
}