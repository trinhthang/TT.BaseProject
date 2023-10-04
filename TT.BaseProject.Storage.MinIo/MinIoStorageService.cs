using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.Storage.MinIo
{
    public class MinIoStorageService : BaseStorageService, IMinIoStorageService
    {
        private readonly ILogger _log;
        private readonly IAmazonS3 _s3 = null;
        private readonly IAmazonS3 _s3Temp = null;

        public MinIoStorageService(
            ILogger<MinIoStorageService> log,
            IOptions<StorageConfig> storageConfig) : base(storageConfig)
        {
            _log = log;

            _s3 = this.CreateProvider(_storageConfig.MinIO.Real);

            _s3Temp = this.CreateProvider(_storageConfig.MinIO.Temp);
        }

        protected virtual IAmazonS3 CreateProvider(StorageMinIoBucketConfig fileConfig)
        {
            var fileConfigStorage = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                ServiceURL = fileConfig.ServiceURL,
                ForcePathStyle = true //Cần phải set thông tin này để SDK tương thích với hệ thống
            };

            return new AmazonS3Client(fileConfig.AccessKey, fileConfig.SecretKey, fileConfigStorage);
        }

        protected override string GetPath(StorageFileType type, string name, object folderId)
        {
            var result = base.GetPath(type, name, folderId);

            if (type != StorageFileType.Temp && type != StorageFileType.Attachment && type != StorageFileType.Upload)
            {
                result = string.Format("{0}/{1}", GetBucketRealConfig().Version, result);
            }

            return result.ToLower();
        }

        protected virtual StorageMinIoBucketConfig GetBucketRealConfig()
        {
            return _storageConfig.MinIO.Real;
        }

        protected override string GetTempPath(string fileName)
        {
            return Path.Combine(this.GetRootPath(), string.Format(_storageConfig.MinIO.Temp.Format, fileName));
        }

        protected virtual string GetBucketName(StorageFileType type)
        {
            if (type == StorageFileType.Temp)
            {
                return _storageConfig.MinIO.Temp.BucketName;
            }

            return _storageConfig.MinIO.Real.BucketName;
        }

        public async Task CopyTempToRealAsync(string tempName, StorageFileType toType, string toName, object folderId)
        {
            if (_storageConfig.MinIO.Real.ServiceURL == _storageConfig.MinIO.Temp.ServiceURL)
            {
                var tempKey = GetPath(StorageFileType.Temp, tempName, null);
                var desKey = GetPath(toType, toName, folderId);
                string tempBucket = this.GetBucketName(StorageFileType.Temp),
                        desBucket = this.GetBucketName(toType);

                _log.LogInformation($"S3StorageService CopyTempToRealAsync tempBucket {tempBucket}, tempKey {tempKey}, desBucket {desBucket}, desKey {desKey}");

                await _s3.CopyObjectAsync(tempBucket, tempKey, desBucket, desKey);
            }
            else
            {
                using (var file = await this.Get(StorageFileType.Temp, name: tempName))
                {
                    await this.SaveAsync(toType, toName, file, folderId: folderId);
                }
            }
        }

        public async Task DeleteAsync(StorageFileType type, string name, object folderId = null)
        {
            var requestParam = new DeleteObjectRequest()
            {
                BucketName = this.GetBucketName(type),
                Key = GetPath(type, name, folderId)
            };

            _log.LogInformation($"S3StorageService DeleteAsync Bucket {requestParam.BucketName}, path {requestParam.Key}");

            var s3 = this.GetProvider(type);
            var result = await s3.DeleteObjectAsync(requestParam);

            if (result.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception($"{result.HttpStatusCode}");
            }
        }

        public virtual async Task<MemoryStream> Get(StorageFileType type, string name = null)
        {
            var path = this.GetPath(type, name, null);
            var requestParam = new GetObjectRequest()
            {
                BucketName = this.GetBucketName(type),
                Key = path
            };

            MemoryStream result = null;
            try
            {
                result = await this.ReadFile(type, requestParam);
            }
            catch (Exception ex)
            {
                _log.LogError($"S3StorageService ReadFile Exception {requestParam.BucketName} - {requestParam.Key}, Message {ex.Message}");
            }

            return result;
        }

        private IAmazonS3 GetProvider(StorageFileType type)
        {
            var s3 = type == StorageFileType.Temp ? _s3Temp : _s3;
            return s3;
        }

        protected virtual async Task<MemoryStream> ReadFile(StorageFileType type, GetObjectRequest requestParam)
        {
            _log.LogInformation($"S3StorageService Get Bucket {requestParam.BucketName}, path {requestParam.Key}");

            var s3 = this.GetProvider(type);
            var result = await s3.GetObjectAsync(requestParam);
            if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            MemoryStream memStream = new MemoryStream();
            result.ResponseStream.CopyTo(memStream);
            result.ResponseStream.Dispose();
            memStream.Position = 0;

            return memStream;
        }

        public virtual async Task SaveAsync(StorageFileType type, string name, Stream content, object folderId = null, string contentType = "text/plain")
        {
            var requestParam = new PutObjectRequest()
            {
                BucketName = this.GetBucketName(type),
                Key = GetPath(type, name, folderId),
                InputStream = content,
                ContentType = contentType
            };

            _log.LogInformation($"S3StorageService Save Bucket {requestParam.BucketName}, path {requestParam.Key}");

            requestParam.Metadata.Add("x-amz-meta-title", name);
            var s3 = this.GetProvider(type);
            var result = await s3.PutObjectAsync(requestParam);

            if (result.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception($"{result.HttpStatusCode}");
            }
        }

        public async Task SaveAsync(StorageFileType type, string name, string content, object folderId = null)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(content);
                stream.Write(byteData, 0, byteData.Length);
                await this.SaveAsync(type, name, stream, folderId);
            }
        }
        public async Task<List<string>> GetFileNamesAsync(StorageFileType type, string subFolder = null)
        {
            var path = this.GetPath(type, subFolder, null);
            var requestParam = new ListObjectsRequest()
            {
                BucketName = this.GetBucketName(type),
                Prefix = path
            };

            _log.LogInformation($"S3StorageService GetFileNamesAsync Bucket {requestParam.BucketName}, path {requestParam.Prefix}");

            var s3 = this.GetProvider(type);
            var result = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                var response = await s3.ListObjectsAsync(requestParam);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    break;
                }

                var temp = response.S3Objects.Select(n => n.Key).ToList();
                result.AddRange(temp);

                if (string.IsNullOrEmpty(response.NextMarker))
                {
                    break;
                }
                requestParam.Marker = response.NextMarker;
            }

            return result;
        }

        protected override string GetRootPath()
        {
            return "";
        }

        protected override string GetDefaultFolder()
        {
            var folder = base.GetDefaultFolder();
            return folder;
        }

        public async Task<bool> ExistAsync(StorageFileType type, string name = null, object folderId = null)
        {
            var file = await this.GetAsync(type, name, folderId);
            if (file != null && file.Length > 0)
            {
                return true;
            }

            return false;
        }

        public override async Task<MemoryStream> GetAsync(StorageFileType type, string name = null, object folderId = null)
        {
            var path = this.GetPath(type, name, folderId);
            var requestParam = new GetObjectRequest()
            {
                BucketName = this.GetBucketName(type),
                Key = path
            };

            MemoryStream result = null;
            try
            {
                result = await this.ReadFile(type, requestParam);
            }
            catch (Exception ex)
            {
                _log.LogError($"S3StorageService ReadFile Exception {requestParam.BucketName} - {requestParam.Key}, Message {ex.Message}");
            }

            return result;
        }
    }
}
