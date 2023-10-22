
namespace TT.BaseProject.Cache
{
    /// <summary>
    /// Xử lý memcache
    /// </summary>
    public interface IInvalidMemoryCacheService
    {
        /// <summary>
        /// Gửi thông báo hủy cache mem cho các host khác
        /// </summary>
        Task InvalidAsync(string key);

        /// <summary>
        /// Thực hiện lắng nghe thông báo hủy mem cache từ host khác
        /// </summary>
        void StartSubcribe();
    }
}