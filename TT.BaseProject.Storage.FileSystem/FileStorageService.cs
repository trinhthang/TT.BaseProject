﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Cache;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.Storage.FileSystem
{
    public class FileStorageService : BaseStorageService, IFileStorageService
    {
        private const string FILE_CUSTOM_PATH = "FILE_CUSTOM_PATH";

        public FileStorageService(IOptions<StorageConfig> storageConfig, ICacheService cacheService) : base(storageConfig, cacheService)
        {
        }

        protected override string GetRootPath()
        {
            string customPath = Environment.GetEnvironmentVariable(FILE_CUSTOM_PATH) ?? "";
            string basePath = AppContext.BaseDirectory;
            string rootPath = Path.Combine(basePath, customPath, "Stores");
            return rootPath;
        }

        public async Task CopyTempToRealAsync(string tempName, StorageFileType toType, string toName, object folderId)
        {
            var tempPath = this.GetPath(StorageFileType.Temp, tempName, null);
            if (System.IO.File.Exists(tempPath))
            {
                var toPath = this.GetPath(toType, toName, folderId);

                this.CreateDirectoryIfNotExist(toPath);

                System.IO.File.Copy(tempPath, toPath);

                System.IO.File.Delete(tempPath);
            }
        }

        public async Task DeleteAsync(StorageFileType type, string name, object folderId = null)
        {
            if (folderId == null)
            {
                var path = this.GetPath(type, name, folderId);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        public override async Task<MemoryStream> GetAsync(StorageFileType type, string name = null, object folderId = null)
        {
            string path = this.GetPath(type, name, folderId);

            if (System.IO.File.Exists(path))
            {
                return new MemoryStream(System.IO.File.ReadAllBytes(path));
            }

            return null;
        }

        public async Task<List<string>> GetFileNamesAsync(StorageFileType type, string subFolder = null)
        {
            var result = new List<string>();
            string path = this.GetPath(type, subFolder, null);

            if (Directory.Exists(path))
            {
                result = Directory.GetFiles(path).Select(n => Path.GetFileName(n)).ToList();
            }

            return result;
        }

        public async Task SaveAsync(StorageFileType type, string name, Stream content, object folderId = null, string contentType = "text/plain")
        {
            var path = this.GetPath(type, name, folderId);

            this.CreateDirectoryIfNotExist(path);

            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                content.Seek(0, SeekOrigin.Begin);
                await content.CopyToAsync(file);
            }
        }

        public async Task SaveAsync(StorageFileType type, string name, string content, object folderId = null)
        {
            var path = this.GetPath(type, name, folderId);

            this.CreateDirectoryIfNotExist(path);

            System.IO.File.WriteAllText(path, content);
        }

        private void CreateDirectoryIfNotExist(string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public async Task<List<string>> GetDirectories(StorageFileType type, string subFolder = null)
        {
            var result = new List<string>();
            string path = this.GetPath(type, subFolder, null);

            if (Directory.Exists(path))
            {
                var folders = Directory.GetDirectories(path);
                foreach (var item in folders)
                {
                    result.Add(item.Split('/').Last());
                }
            }

            return result;
        }

        public async Task<List<string>> GetDirectoriesAsync(StorageFileType type, string subFolder = null)
        {
            var result = new List<string>();
            string path = this.GetPath(type, subFolder, null);

            if (Directory.Exists(path))
            {
                var folders = Directory.GetDirectories(path);
                foreach (var item in folders)
                {
                    result.Add(item.Split('/').Last());
                }
            }

            return result;
        }

        public async Task<bool> ExistAsync(StorageFileType type, string name = null, object folderId = null)
        {
            string path = this.GetPath(type, name, folderId);

            if (System.IO.File.Exists(path))
            {
                return true;
            }

            return false;
        }
    }
}
