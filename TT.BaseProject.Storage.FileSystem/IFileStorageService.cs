using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.Storage.FileSystem
{
    public interface IFileStorageService : IStorageService
    {
        Task<List<string>> GetDirectoriesAsync(StorageFileType type, string subFolder = null);
    }
}