using TT.BaseProject.Cache.Models;

namespace TT.BaseProject.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Gán cache
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu lưu trữ</typeparam>
        /// <param name="param">Tham số</param>
        /// <param name="data">Dữ liệu</param>
        /// <returns>Trả về expired time (seconds)</returns>
        int Set<T>(CacheParam param, T data);

        /// <summary>
        /// Đọc cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="removeAfterGet">Xóa luôn sau khi đọc ra</param>
        T Get<T>(CacheParam param, bool removeAfterGet = false);

        /// <summary>
        /// Xóa cache
        /// </summary>
        /// <param name="param"></param>
        void Remove(CacheParam param);

        /// <summary>
        /// Try Get cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <param name="result"></param>
        /// <param name="removeAfterGet"></param>
        /// <returns></returns>
        bool TryGet<T>(CacheParam param, out T result, bool removeAfterGet = false);
    }
}