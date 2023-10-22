using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache.Models
{
    public class CacheConfig
    {
        public List<string> DistGroups { get; set; }

        public Dictionary<string, CacheItemConfig> Items { get; set; }

        public MemoryCacheConfig Mem { get; set; }
    }

    public class MemoryCacheConfig
    {
        public MemoryCacheInvalidConfig InvalidCache { get; set; }
    }

    public class MemoryCacheInvalidConfig
    {
        public string Type { get; set; }

        public InvalidMemoryCacheRedisConfig Redis { get; set; }
    }

    public class InvalidMemoryCacheRedisConfig
    {
        public string Connection { get; set; }

        public string Channel { get; set; }
    }
}
