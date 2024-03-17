using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Crud;
using TT.BaseProject.Domain.Filter;

namespace TT.BaseProject.Domain.Base
{
    public interface IBaseRepo
    {
        IDbConnection GetOpenConnection();

        void CloseConnection(IDbConnection cnn);

        Task<List<T>> GetAsync<T>(string field, object value, string op = "=");
        Task<List<T>> GetAsync<T>(IDbConnection cnn, string field, object value, string op = "=", string columns = "*");
        Task<List<T>> GetAsync<T>(IDbTransaction transaction, string field, object value, string op = "=");
        Task<List<T>> GetAsync<T>();
        Task<List<T>> GetAsync<T>(IDbConnection cnn);
        Task<List<T>> GetAsync<T>(List<FilterItem> filters, string? sort = null, int skip = 0, int? take = null, string? emptyFilter = null);

        T GetById<T>(object id);
        Task<T> GetByIdAsync<T>(object id);
        Task<T> GetByIdAsync<T>(IDbConnection cnn, object id);
        Task<T> GetByIdAsync<T>(IDbConnection cnn, Type type, object id);
        List<T> GetByIds<T>(IList ids);
        Task<List<T>> GetByIdsAsync<T>(IList ids);

        Task<object> InsertAsync(object entity);
        Task<object> InsertAsync(IDbConnection cnn, object entity);
        Task<object> InsertAsync(IDbTransaction transaction, object entity);
        Task<object> InsertAsync(IDbTransaction transaction, Type type, object entity);

        Task InsertBatchAsync(IDbConnection cnn, IList entities);
        Task InsertBatchAsync(IDbConnection cnn, IList entities, Type type);
        Task InsertBatchAsync(IDbTransaction transaction, IList entities);
        Task InsertBatchAsync(IDbTransaction transaction, IList entities, Type type);

        bool Update(IDbConnection cnn, object entity, string fields = null);
        Task<bool> UpdateAsync(object entity, string fields = null);
        Task<bool> UpdateAsync(IDbTransaction transaction, object entity, string fields = null);
        Task<bool> UpdateAsync(IDbTransaction transaction, Type type, object entity, string fields = null);
        Task<bool> UpdateAsync(IDbConnection cnn, object entity, string fields = null);
        Task<bool> UpdateAsync(IDbConnection cnn, Type type, object entity, string fields = null);

        Task<bool> DeleteAsync(object entity);
        Task<bool> DeleteAsync(IDbTransaction transaction, object entity);
        Task<bool> DeleteAsync(IDbTransaction transaction, Type type, object entity);
        Task<bool> DeleteAsync(IDbConnection cnn, object entity);

        Task<IList> GetDynamicAsync(Type type, string columns);
        Task<IList> GetDynamicAsync(Type type, string columns, string sort);
        Task<IList> GetDynamicAsync(IDbConnection cnn, Type type, string columns);
        Task<IList> GetDynamicAsync(Type type, string columns, string field, object value, string op = "=");
        Task<List<T>> GetDynamicAsync<T>(Type type, string columns, string field, object value, string op = "=");
        Task<List<T>> GetDynamicAsync<T>(string columns);
        Task<List<T>> GetDynamicAsync<T>(string columns, string field, object value, string op = "=");
        List<T> GetDynamic<T>(string columns, string field, object value, string op = "=");

        Task<PagingResult> GetPagingAsync(Type type, string sort, int skip, int take, string columns, string filter = null, string emptyFilter = null);
        Task<PagingSummaryResult> GetPagingSummaryAsync(Type type, string columns, string filter = null);
        Task<IList> GetComboboxPagingAsync(Type type, string sort, int skip, int take, string columns, string filter, string selectedItem);
        Task<IList> GetTreeAsync<T>(string columns, string sort);

        bool UpdateMulti(IDbConnection cnn, Type type, string fields, object value, IList ids);
        Task<bool> UpdateMultiAsync(IDbConnection cnn, Type type, string fields, object value, IList ids);

        Task<int> UpdatesAsync(IList entities, Type type = null, string fields = null);
        Task<int> UpdatesAsync(IDbConnection cnn, IList entities, Type type = null, string fields = null);
        Task<int> UpdatesAsync(IDbTransaction transaction, IList entities, Type type = null, string fields = null);

        Task InsertsAsync(IList entities);
        Task InsertsAsync(IList entities, IDbConnection cnn);
        Task InsertsAsync(IList entities, IDbTransaction transaction);
    }
}
