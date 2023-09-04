using System.Collections;
using System.Data;

namespace TT.BaseProject.MySql
{
    public interface IDatabaseProvider
    {
        string GetConnectionString();

        IDbConnection GetConnection();

        IDbConnection GetOpenConnection();

        void CloseConnection(IDbConnection connection);

        int ExecuteNonQueryText(IDbConnection cnn, string commandText, object param);

        Task<List<T>> ExecuteQueryObjectAsync<T>(string storeName, object param = null);
        Task<List<T>> ExecuteQueryObjectAsync<T>(IDbConnection cnn, string storeName, object param = null);

        Task<int> ExecuteNonQueryObjectAsync(string storeName, object param = null);
        Task<int> ExecuteNonQueryObjectAsync(IDbConnection cnn, string storeName, object param = null);

        Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string commandText);
        Task<int> ExecuteNonQueryTextAsync(string commandText, Dictionary<string, object> param, int? timeout = null);
        Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param, int? timeout = null);
        Task<int> ExecuteNonQueryTextAsync(IDbTransaction transaction, string commandText, Dictionary<string, object> param);

        Task<int> ExecuteNonQueryTextAsync(string commandText, object param);
        Task<int> ExecuteNonQueryTextAsync(IDbConnection cnn, string commandText, object param);
        Task<int> ExecuteNonQueryTextAsync(IDbTransaction transaction, string commandText, object param);

        Task<object> ExecuteScalarTextAsync(string commandText, Dictionary<string, object> param);
        Task<object> ExecuteScalarTextAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param);
        Task<object> ExecuteScalarTextAsync(IDbTransaction transaction, string commandText, Dictionary<string, object> param);
        Task<object> ExecuteScalarTextAsync(string commandText, object param);
        Task<object> ExecuteScalarTextAsync(IDbConnection cnn, string commandText, object param);
        Task<object> ExecuteScalarTextAsync(IDbTransaction transaction, string commandText, object param);

        Task<List<T>> QueryAsync<T>(string commandText, Dictionary<string, object> param);
        Task<List<T>> QueryAsync<T>(IDbConnection cnn, string commandText, Dictionary<string, object> param);
        Task<List<T>> QueryAsync<T>(IDbTransaction transaction, string commandText, Dictionary<string, object> param);
        List<T> Query<T>(string commandText, Dictionary<string, object> param);
        List<T> Query<T>(IDbConnection cnn, string commandText, Dictionary<string, object> param);
        //List<T> Query<T>(IDbTransaction transaction, string commandText, Dictionary<string, object> param);

        Task<IList> QueryAsync(Type type, string commandText, Dictionary<string, object> param);
        Task<IList> QueryAsync(IDbConnection cnn, Type type, string commandText, Dictionary<string, object> param);
        Task<IList> QueryAsync(IDbTransaction transaction, Type type, string commandText, Dictionary<string, object> param);
        Task<IList> QueryAsync(string commandText, Dictionary<string, object> param);
        Task<IList> QueryAsync(IDbConnection cnn, string commandText, Dictionary<string, object> param);

        Task<List<T>> QueryWithCommandTypeAsync<T>(IDbConnection cnn, string commandText, CommandType pCommandType, Dictionary<string, object> param);
        Task<List<T>> QueryWithCommandTypeWithTranAsync<T>(IDbTransaction transaction, string commandText, CommandType pCommandType, Dictionary<string, object> param);

    }
}
