using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Common;

namespace TT.BaseProject.Library.Service
{
    public class SerializerService : ISerializerService
    {
        public string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T DeserializeObject<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        public object DeserializeObject(string s, Type type)
        {
            return JsonConvert.DeserializeObject(s, type);
        }
    }
}
