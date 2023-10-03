using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Config
{
    public class StorageConfig
    {
        public List<string> UploadAllowExtensions { get; set; }

        public decimal? UploadMaxSizeMB { get; set; }

        public string Type { get; set; }

        public StorageFileConfig File { get; set; }

        public StorageMinIOConfig MinIO { get; set; }
    }

    public class StorageFileConfig
    {
        public string Real { get; set; }

        public string Temp { get; set; }
    }

    public class StorageMinIOConfig
    {
        public StorageMinIoBucketConfig Real { get; set; }

        public StorageMinIoBucketConfig Temp { get; set; }
    }

    public class StorageMinIoBucketConfig
    {
        ///<summary>
        /// Endpoint kết nối TT Storage
        ///</summary>
        public string ServiceURL { get; set; }

        ///<summary>
        /// UserName
        ///</summary>
        public string AccessKey { get; set; }

        ///<summary>
        /// Password
        ///</summary>
        public string SecretKey { get; set; }

        ///<summary>
        /// Tên bucket lưu trữ object được chỉ định cho tài khoản này
        /// Để thêm path (subfolder) thì cần thêm /pathname vào sau bucket
        /// Công thức BucketName/path1/path2/...pathn
        /// VD: tt/development/folder1
        /// VD: tt/test/folder1
        ///</summary>
        public string BucketName { get; set; }

        ///<summary>
        /// Tùy chỉnh thư mục lưu trữ file
        ///</summary>
        public string Format { get; set; }

        ///<summary>
        /// Tùy chỉnh version dùng cho minio đáp ứng trường hợp multi version và chung bucket
        ///</summary>
        public string Version { get; set; }
    }
}
