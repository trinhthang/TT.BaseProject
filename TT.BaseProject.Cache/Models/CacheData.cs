using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache.Models
{
    /// <summary>
    /// Dữ liệu lưu vào cache
    /// Wrap để phân biệt không có trong cache và lưu giá trị default(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheData<T>
    {
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public T Value { get; set; }
    }
}
