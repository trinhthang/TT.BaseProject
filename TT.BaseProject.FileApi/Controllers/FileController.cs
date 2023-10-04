using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TT.BaseProject.Domain.Config;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Storage;
using TT.BaseProject.Storage.Enums;

namespace TT.BaseProject.FileApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //TODO [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IContextService _contextService;
        private readonly StorageConfig _storageConfig;

        public FileController(IStorageService storageService, IContextService contextService, IOptions<StorageConfig> storageConfig)
        {
            _storageService = storageService;
            _contextService = contextService;
            _storageConfig = storageConfig.Value;
        }

        [HttpGet]
        [HttpGet("{type}")]
        [HttpGet("{type}/{file}")]
        [HttpGet("{type}/{file}/{name}")]
        [HttpGet("{type}/{file}/{name}/{dbid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string file, StorageFileType type, int? dbid, string name)
        {
            string contentType = _storageService.GetContentType(Path.GetExtension(file));
            var result = await GetFile(file, type, dbid, name, contentType);
            return result;
        }

        [HttpGet("download")]
        [HttpGet("download/{type}")]
        [HttpGet("download/{type}/{file}")]
        [HttpGet("download/{type}/{file}/{name}")]
        [HttpGet("download/{type}/{dbid}/{name}/{file}")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(string file, StorageFileType type, int? dbid, string name)
        {
            var result = await GetFile(file, type, dbid, name, "application/octect-stream");
            return result;
        }

        [HttpPost]
        [RequestSizeLimit(100000000)]
        public async Task<IActionResult> UploadFile([FromForm] StorageFileType type, [FromForm] IFormFile file)
        {
            var err = ValidateUpload(type, file);
            if (!string.IsNullOrEmpty(err))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, err);
            }
            else
            {
                var context = _contextService.Get();
                string fileKey = string.Format(Guid.NewGuid().ToString());
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{fileKey}.{fileExtension}";
                string fileType = _storageService.GetContentType(fileExtension);

                using (var stream = file.OpenReadStream())
                {
                    //upload temp only
                    await _storageService.SaveAsync(StorageFileType.Temp, fileName, stream, context.UserId, contentType: fileType);
                }

                return Ok(fileName);
            }
        }

        [HttpPost("multi")]
        [RequestSizeLimit(100000000)]
        public async Task<IActionResult> UploadFiles([FromForm] StorageFileType type, [FromForm] List<IFormFile> file)
        {
            int validCount = 0;
            int errorCount = 0;
            var context = _contextService.Get();
            var temps = new List<object>();
            for (int i = 0; i < file.Count; i++)
            {
                var item = file[i];
                var err = ValidateUpload(type, item);
                if (!string.IsNullOrEmpty(err))
                {
                    temps.Add(new { error = err });
                    errorCount++;
                }
                else
                {
                    validCount++;
                    string fileKey = string.Format(Guid.NewGuid().ToString());
                    string fileExtension = Path.GetExtension(item.FileName);
                    string fileName = $"{fileKey}.{fileExtension}";
                    string fileType = _storageService.GetContentType(fileExtension);

                    using (var stream = item.OpenReadStream())
                    {
                        //upload temp only
                        await _storageService.SaveAsync(StorageFileType.Temp, fileName, stream, context.UserId, contentType: fileType);
                    }

                    temps.Add(new { name = fileName, originName = item.FileName });
                }
            }

            return Ok(temps);
        }

        private string ValidateUpload(StorageFileType type, IFormFile file)
        {
            var data = file.FileName.Split(".");

            if (data.Length < 2)
            {
                return "File name invalid";
            }

            var ext = data.Last().ToLower();
            if (_storageConfig.UploadAllowExtensions != null && !_storageConfig.UploadAllowExtensions.Contains(ext))
            {
                return "File extension invalid";
            }

            if (_storageConfig.UploadMaxSizeMB.HasValue && file.Length > _storageConfig.UploadMaxSizeMB.Value * 1024 * 1024)
            {
                return $"File size bigger than {_storageConfig.UploadMaxSizeMB} MB";
            }

            return null;
        }

        private async Task<IActionResult> GetFile(string file, StorageFileType type, int? dbid, string name, string contentType)
        {
            var stream = await _storageService.GetAsync(type, file, dbid);
            if (stream == null)
            {
                return NotFound("Khong tim thay file");
            }
            if (contentType.StartsWith("image/"))
            {
                var result = File(stream, contentType);
                return result;
            }
            else if (contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Response.Headers.Add("Content-Disposition", String.Format("inline; title={0}; filename={1}", Path.GetFileNameWithoutExtension(name), name));
                var result = new FileStreamResult(stream, contentType);
                return result;
            }
            else
            {
                string fileName;
                if (!string.IsNullOrEmpty(name))
                {
                    fileName = Path.GetFileNameWithoutExtension(name) + Path.GetExtension(name);
                }
                else
                {
                    fileName = file;
                }

                var result = File(stream, contentType, fileName);
                return result;
            }
        }
    }
}
