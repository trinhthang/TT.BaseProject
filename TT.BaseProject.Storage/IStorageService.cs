using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.Storage
{
    public interface IStorageService
    {
        Task<MemoryStream> GetAsync(StorageFileType type, string name = null, object databaseId = null);
        Task<string> GetStringAsync(StorageFileType type, string name = null, object databaseId = null);
        Task SaveAsync(StorageFileType type, string name, Stream content, object databaseId = null, string contentType = "text/plain");
        Task SaveAsync(StorageFileType type, string name, string content, object databaseId = null);
        Task DeleteAsync(StorageFileType type, string name, object databaseId = null);
        Task CopyTempToRealAsync(string tempName, StorageFileType toType, string toName, object databaseId);
        Task<List<string>> GetFileNamesAsync(StorageFileType type, string subFolder = null);
        Task<bool> ExistAsync(StorageFileType type, string name = null, object databaseId = null);
        string GetContentType(string extension);
    }
}
