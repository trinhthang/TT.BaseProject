using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.Query;

namespace TT.BaseProject.Domain.Base
{
    public interface IBusinessBaseRepo : IBaseRepo
    {
        void SetContextData(ContextData context);

        Task SubmitAsync(List<SubmitModel> data);
        Task SubmitAsync(IDbConnection cnn, List<SubmitModel> data);
        Task SubmitAsync(IDbTransaction transaction, List<SubmitModel> data);

        Task<bool> DeletesAsync(IDbConnection cnn, Type type, string field, IList values);
        Task<bool> DeletesAsync(IDbTransaction transaction, Type type, string field, IList values);

        Task<List<T>> QueryAsync<T>(string commandText, Dictionary<string, object> param);

        Task<int> ExecuteStoreAsync(IDbConnection cnn, string sql, Dictionary<string, object> param);

        /// <summary>
        /// Lấy dữ liệu chi tiết theo master id
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="config"></param>
        /// <param name="returnType"></param>
        /// <param name="masterId"></param>
        /// <returns></returns>
        Task<IList> GetRefByMasterAsync(IDbConnection cnn, IMasterRefAttribute config, Type returnType, object masterId);

        /// <summary>
        /// Kiểm tra dữ liệu bị trùng
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="model"></param>
        /// <param name="keyFields"></param>
        /// <param name="uniqueFields"></param>
        /// <returns></returns>
        Task<IList> HasDuplicateAsync(IDbConnection cnn, object model, List<PropertyInfo> keyFields, List<PropertyInfo> uniqueFields);

    }
}
