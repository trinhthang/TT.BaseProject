using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Common
{
    public interface ISerializerService
    {
        string SerializeObject(object obj);

        T DeserializeObject<T>(string s);

        object DeserializeObject(string s, Type type);
    }
}
