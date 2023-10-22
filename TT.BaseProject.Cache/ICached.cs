using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Cache
{
    public interface ICached
    {
        void Set<T>(string key, T data, TimeSpan? expired = null);

        T Get<T>(string key, bool removeAfterGet = false);

        void Remove(string key);
    }
}
