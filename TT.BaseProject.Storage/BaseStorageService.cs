using Microsoft.Extensions.Options;
using TT.BaseProject.Cache;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Constant;
using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.Storage
{
    public class BaseStorageService
    {
        protected readonly StorageConfig _storageConfig;
        protected readonly ICacheService _cacheService;
        private readonly string _defaultFolder;

        public BaseStorageService(IOptions<StorageConfig> storageConfig, ICacheService cacheService)
        {
            _storageConfig = storageConfig.Value;
            _cacheService = cacheService;
            _defaultFolder = this.GetDefaultFolder();
        }

        protected virtual string GetPath(StorageFileType type, string name, object folderId)
        {
            //Xử lý chống truy cập trái phép
            string fileName = string.IsNullOrEmpty(name) ? "" : Path.GetFileName(name);
            string path;

            /*
             * Nếu sử dụng trên AWS S3 thì sẽ không ghép thêm path Environment vào do trên đó đã lưu trữ rồi mà không muốn convert vào thư mục theo Env nữa
             * Các môi trường khác MinIO hoặc File (trên development) thì sẽ lưu riêng theo từng môi trường
             * //var envName = GetEnvironmentName()
             */
            switch (type)
            {
                case StorageFileType.Temp:
                    path = this.GetTempPath(fileName);
                    break;
                case StorageFileType.Attachment:
                    path = Path.Combine(this.GetRootPath(), $"attachment/{name}");
                    break;
                case StorageFileType.Upload:
                    path = Path.Combine(this.GetRootPath(), $"upload/{name}");
                    break;
                default:
                    /*
                    * Mặc định thư mục lưu file là {typeName}/{fileName}
                    * Với file lưu theo db là {folderId}/{typeName}/{fileName}
                    */
                    path = Path.Combine(this.GetStorePath(folderId), type.ToString(), fileName);
                    break;
            }

            return path.Replace(@"\", "/");
        }

        protected virtual string GetTempPath(string fileName)
        {
            return Path.Combine(this.GetRootPath(), "temp", fileName);
        }

        protected virtual string GetStorePath(object folderId)
        {
            string subPath;
            if (folderId != null)
            {
                subPath = Path.Combine("Database", folderId.ToString());
            }
            else
            {
                subPath = _defaultFolder;
            }
            return Path.Combine(this.GetRootPath(), subPath);
        }

        protected virtual string GetDefaultFolder()
        {
            return "";
        }

        protected virtual string GetRootPath()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetStringAsync(StorageFileType type, string name = null, object folderId = null)
        {
            string content = await this.GetFileStringAsync(type, name, folderId);
            return content;
        }

        public virtual async Task<string> GetFileStringAsync(StorageFileType type, string name = null, object folderId = null)
        {
            string content = "";
            using (var stream = await this.GetAsync(type, name, folderId))
            {
                StreamReader reader = new StreamReader(stream);
                content = await reader.ReadToEndAsync();
            };

            return content;
        }

        public virtual async Task<MemoryStream> GetAsync(StorageFileType type, string name = null, object folderId = null)
        {
            throw new NotImplementedException();
        }

        public string GetContentType(string extension)
        {
            string type;
            switch (extension?.ToLower())
            {
                case ".xls":
                    type = ContentType.ContentType_Excel.ToString();
                    break;
                case ".xlsx":
                    type = ContentType.ContentType_ExcelOpenXML.ToString();
                    break;
                case ".doc":
                case ".docx":
                    type = "application/msword";
                    break;
                case ".pdf":
                    type = "application/pdf";
                    break;
                case ".zip":
                    type = "application/x-zip-compressed";
                    break;
                case ".png":
                    type = "image/png";
                    break;
                case ".gif":
                    type = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    type = "image/jpg";
                    break;
                case ".tif":
                    type = "image/tif";
                    break;
                case ".svg":
                    type = "image/svg+xml";
                    break;
                case ".css":
                    type = "text/css";
                    break;
                case ".js":
                    type = "text/js";
                    break;
                default:
                    type = "text/plain";
                    break;
            }

            return type;
        }
    }
}
