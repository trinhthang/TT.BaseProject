using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Common
{
    public interface ITypeService
    {
        bool IsList(Type type);

        List<PropertyInfo> GetKeyFields(Type type);

        PropertyInfo GetKeyField(Type type);

        Type GetTypeInList(Type listType);

        Dictionary<PropertyInfo, TAttribute> GetPropertys<TAttribute>(Type type)
            where TAttribute : Attribute;

        string GetTableName(Type type);

        string GetMasterTableName(Type type);

        List<string> GetTableColumns(Type type);

        TDes MapData<TDes>(object origin);

        object MapData(object origin, Type desType);

        PropertyInfo GetKey(Type type);
    }
}
