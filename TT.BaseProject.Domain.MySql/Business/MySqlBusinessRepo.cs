using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Base;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.Query;
using TT.BaseProject.MySql;

namespace TT.BaseProject.Domain.MySql.Business
{
    public class MySqlBusinessRepo : MySqlRepo, IBusinessBaseRepo
    {
        private IContextService _contextService;
        private readonly ConnectionConfig _connectionConfig;

        private ContextData _contextData = null;

        public MySqlBusinessRepo(IOptions<ConnectionConfig> connectionConfig, IServiceProvider serviceProvider) : base(connectionConfig.Value.Business, serviceProvider)
        {
            _connectionConfig = connectionConfig.Value;
            SetContextService();
        }

        protected virtual void SetContextService()
        {
            _contextService = _serviceProvider.GetRequiredService<IContextService>();
        }

        public void SetContextData(ContextData contextData)
        {
            _contextData = contextData;

            //reset provider để khởi tạo lại theo context mới
            _databaseProvider = null;
        }

        protected override IDatabaseProvider CreateProvider(string connectionString)
        {
            var connection = connectionString ?? this.GetConnectionString();
            return new MySqlProvider(connection);
        }

        protected string GetConnectionString()
        {
            var context = _contextData ?? _contextService.Get();
            if (context == null)
            {
                //TODO Tạm thời rem lại context, để sau khi bổ sung authen author thì bổ sung vào
                //throw new Exception($"Not found context");
            }

            #region Lấy Connection trong context khi là ứng dụng multi tenant
            #endregion

            return _connectionConfig.Business;
        }

        public async Task<List<T>> QueryAsync<T>(string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            List<T> data = null;

            try
            {
                cnn = this.Provider.GetOpenConnection();
                data = await this.Provider.QueryWithCommandTypeAsync<T>(cnn, commandText, CommandType.Text, param);
            }
            finally
            {
                this.Provider.CloseConnection(cnn);
            }

            return data;
        }

        public async Task<int> ExecuteStoreAsync(IDbConnection cnn, string sql, Dictionary<string, object> param)
        {
            return await this.Provider.ExecuteNonQueryTextAsync(cnn, sql, param);
        }

        public async Task<IList> GetRefByMasterAsync(IDbConnection cnn, IMasterRefAttribute config, Type returnType, object masterId)
        {
            var table = this.GetTableName(config.Type);
            var query = $"SELECT * FROM {table} WHERE {config.MasterKeyField} = @masterId";
            var dic = new Dictionary<string, object>()
            {
                {"masterId", masterId }
            };

            return await this.Provider.QueryAsync(cnn, returnType, query, dic);
        }

        public async Task<IList> HasDuplicateAsync(IDbConnection cnn, object model, List<PropertyInfo> keyFields, List<PropertyInfo> uniqueFields)
        {
            var type = model.GetType();
            var tableName = this.GetTableName(type);
            var param = new Dictionary<string, object>();
            var action = new List<Task<IList>>();

            for (int i = 0; i < uniqueFields.Count; i++)
            {
                var sb = new StringBuilder($"SELECT * FROM {tableName} WHERE");
                sb.Append(" (");
                var item = uniqueFields[i];
                sb.AppendFormat("{0}=@{0}", item.Name);
                param[item.Name] = item.GetValue(model);
                sb.Append(")");

                if (keyFields != null && keyFields.Count > 0)
                {
                    sb.Append(" AND (");
                    for (int index = 0; index < keyFields.Count; index++)
                    {
                        if (index > 0)
                        {
                            sb.Append(" OR");
                        }

                        var element = keyFields[index];
                        sb.AppendFormat("{0} <> @{0}", element.Name);
                        param[element.Name] = element.GetValue(model);
                    }
                    sb.Append(")");
                }
                sb.Append(" LIMIT 1;");

                action.Add(this.Provider.QueryAsync(cnn, type, sb.ToString(), param));
            }

            var existedRecords = await Task.WhenAll(action);

            return existedRecords;
        }

        public async Task SubmitAsync(List<SubmitModel> data)
        {
            var querys = this.GetSubmitQuery(data);
            foreach (var query in querys)
            {
                await this.Provider.ExecuteNonQueryTextAsync(query.Query, query.Param);
            }
        }

        public async Task SubmitAsync(IDbConnection cnn, List<SubmitModel> data)
        {
            var querys = this.GetSubmitQuery(data);
            foreach (var query in querys)
            {
                await this.Provider.ExecuteNonQueryTextAsync(cnn, query.Query, query.Param);
            }
        }

        public async Task SubmitAsync(IDbTransaction transaction, List<SubmitModel> data)
        {
            var querys = this.GetSubmitQuery(data);
            foreach (var query in querys)
            {
                await this.Provider.ExecuteNonQueryTextAsync(transaction, query.Query, query.Param);
            }
        }

        protected List<SqlQuery> GetSubmitQuery(List<SubmitModel> data)
        {
            List<SqlQuery> result = new List<SqlQuery>(), temp;

            foreach (var item in data)
            {
                var sb = new StringBuilder();
                switch (item.State)
                {
                    case Enum.ModelState.Insert:
                        temp = this.GetSubmitQueryInsert(item);
                        result.AddRange(temp);
                        break;
                    case Enum.ModelState.Update:
                        temp = this.GetSubmitQueryUpdate(item);
                        result.AddRange(temp);
                        break;
                    case Enum.ModelState.Delete:
                        temp = this.GetSubmitQueryDelete(item);
                        result.AddRange(temp);
                        break;
                }
            }

            return result;
        }

        protected List<SqlQuery> GetSubmitQueryInsert(SubmitModel item)
        {
            SqlQuery result = new SqlQuery()
            {
                Param = new Dictionary<string, object>()
            };
            var sb = new StringBuilder();

            var same = true;
            string[] fields = null;
            if (item.Datas.Count > 1)
            {
                string lastKey = null;
                foreach (var row in item.Datas)
                {
                    var temp = string.Join(",", row.Keys.OrderBy(n => n));
                    if (lastKey == null)
                    {
                        lastKey = temp;
                        fields = row.Keys.ToArray();
                    }
                    else if (temp != lastKey)
                    {
                        same = false;
                        break;
                    }
                }
            }

            var valueMap = new Dictionary<object, string>();
            int paramCount = 0;
            string paramField;

            if (same)
            {
                //insert multi
                sb.Append($"INSERT INTO {item.TableName} ({string.Join(",", fields)}) VALUES");
                for (int i = 0; i < item.Datas.Count; i++)
                {
                    var dataItem = item.Datas[i];
                    if (i > 0)
                    {
                        sb.AppendLine(",(");
                    }
                    else
                    {
                        sb.Append("(");
                    }

                    for (int j = 0; j < fields.Length; j++)
                    {
                        //sp field
                        if (j > 0)
                        {
                            sb.Append(",");
                        }

                        //row field
                        var field = fields[j];
                        var value = dataItem[field];

                        if (value == null)
                        {
                            paramField = $"pnu";
                            result.Param[paramField] = value;
                        }
                        else if (valueMap.ContainsKey(value))
                        {
                            paramField = valueMap[value];
                        }
                        else
                        {
                            paramField = $"p{paramCount++}";
                            valueMap[value] = paramField;
                            result.Param[paramField] = value;
                        }

                        sb.Append($"@{paramField}");
                        result.Param[paramField] = value;
                    }
                    sb.AppendLine(")");
                }
                sb.Append(";");
            }
            else
            {
                //insert one row
                for (int i = 0; i < item.Datas.Count; i++)
                {
                    var dataItem = item.Datas[i];
                    fields = dataItem.Keys.ToArray();
                    sb.Append($"INSERT INTO {item.TableName} ({string.Join(",", fields)}) VALUES (");

                    for (int j = 0; j < fields.Length; j++)
                    {
                        //sp field
                        if (j > 0)
                        {
                            sb.Append(",");
                        }

                        //row field
                        var field = fields[j];
                        var value = dataItem[field];

                        if (value == null)
                        {
                            paramField = $"pnu";
                            result.Param[paramField] = value;
                        }
                        else if (valueMap.ContainsKey(value))
                        {
                            paramField = valueMap[value];
                        }
                        else
                        {
                            paramField = $"p{paramCount++}";
                            valueMap[value] = paramField;
                            result.Param[paramField] = value;
                        }

                        sb.Append($"@{paramField}");
                        result.Param[paramField] = value;
                    }
                    sb.AppendLine(");");
                }
            }

            result.Query = sb.ToString();
            return new List<SqlQuery> { result };
        }

        protected List<SqlQuery> GetSubmitQueryUpdate(SubmitModel item)
        {
            SqlQuery result = new SqlQuery()
            {
                Param = new Dictionary<string, object>()
            };
            var sb = new StringBuilder();
            var valueMap = new Dictionary<object, string>();
            int pCount = 0;
            string paramField;

            for (int i = 0; i < item.Datas.Count; i++)
            {
                var data = item.Datas[i];
                int f = 0;
                var sbKeys = new StringBuilder();
                var sbTemp = new StringBuilder();

                foreach (var field in data.Keys)
                {
                    var value = data[field];
                    if (value == null)
                    {
                        paramField = "pnu";
                        result.Param[paramField] = value;
                    }
                    else if (valueMap.ContainsKey(value))
                    {
                        paramField = valueMap[value];
                    }
                    else
                    {
                        paramField = $"p{pCount++}";
                        valueMap[value] = paramField;

                        result.Param[paramField] = value;
                    }

                    if (!item.KeyFields.Contains(field))
                    {
                        if (f > 0)
                        {
                            sbTemp.Append(",");
                        }
                        f++;

                        if (field.Contains("="))
                        {
                            if (field.Contains("#ReplaceValue#"))
                            {
                                sbTemp.Append(field.Replace("#ReplaceValue#", $"@{paramField}"));
                            }
                            else
                            {
                                sbTemp.Append($"{field}@{paramField}");
                            }
                        }
                        else
                        {
                            sbTemp.Append($"{field}=@{paramField}");
                        }
                    }
                    else
                    {
                        if (sbKeys.Length > 0)
                        {
                            sbKeys.Append($" AND {field}=@{paramField}");
                        }
                        else
                        {
                            sbKeys.Append($" {field}=@{paramField}");
                        }
                    }
                }

                if (sbKeys.Length > 0)
                {
                    sb.Append($"UPDATE {item.TableName} SET {sbTemp.ToString()} WHERE {sbKeys.ToString()};");
                }
            }

            result.Query = sb.ToString();
            return new List<SqlQuery> { result };
        }

        protected List<SqlQuery> GetSubmitQueryDelete(SubmitModel item)
        {
            SqlQuery result = new SqlQuery()
            {
                Param = new Dictionary<string, object>()
            };
            var sb = new StringBuilder();
            var valueMap = new Dictionary<object, string>();
            int pCount = 0;
            string paramField;

            for (int i = 0; i < item.Datas.Count; i++)
            {
                var data = item.Datas[i];
                var sbKeys = new StringBuilder();
                foreach (var field in item.KeyFields)
                {
                    if (!data.ContainsKey(field))
                    {
                        sbKeys.Clear();
                        break;
                    }

                    var value = data[field];
                    if (valueMap.ContainsKey(field))
                    {
                        paramField = valueMap[value];
                    }
                    else
                    {
                        paramField = $"p{pCount++}";
                        valueMap[value] = paramField;
                        result.Param[paramField] = value;
                    }

                    if (sb.Length > 0)
                    {
                        sbKeys.Append($" AND {field}=@{paramField}");
                    }
                    else
                    {
                        sbKeys.Append($" {field}=@{paramField}");
                    }
                }

                //có key mới delete
                if (sbKeys.Length > 0)
                {
                    sb.Append($"DELETE FROM {item.TableName} WHERE {sbKeys.ToString()};");
                }
                sb.AppendLine(";");
            }


            result.Query = sb.ToString();
            return new List<SqlQuery> { result };
        }
    }
}
