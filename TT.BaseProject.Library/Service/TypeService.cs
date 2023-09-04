using System.Reflection;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Domain.Attributes;

namespace TT.BaseProject.Library.Service
{
    public class TypeService : ITypeService
    {
        public PropertyInfo GetKey(Type type)
        {
            return type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(KeyAttribute)))?.FirstOrDefault();
        }

        public PropertyInfo GetKeyField(Type type)
        {
            return GetKeyFields(type).FirstOrDefault();
        }

        public List<PropertyInfo> GetKeyFields(Type type)
        {
            var keys = this.GetPropertys<KeyAttribute>(type);
            return keys.Select(n => n.Key).ToList();
        }

        public Dictionary<PropertyInfo, TAttribute> GetPropertys<TAttribute>(Type type) where TAttribute : Attribute
        {
            if (type == null)
            {
                return null;
            }

            var result = new Dictionary<PropertyInfo, TAttribute>();
            var prs = type.GetProperties();
            foreach (var pr in prs)
            {
                var attr = pr.GetCustomAttribute<TAttribute>(true);
                if (attr != null)
                {
                    result.Add(pr, attr);
                }
            }

            return result;
        }

        public string GetTableName(Type type)
        {
            var attr = type.GetCustomAttribute<TableAttribute>();
            if (attr == null)
            {
                return null;
            }

            return attr.Table;
        }

        public string GetMasterTableName(Type type)
        {
            var attr = type.GetCustomAttribute<MasterTableAttribute>();
            if (attr == null)
            {
                return null;
            }

            return attr.Name;
        }

        public List<string> GetTableColumns(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var fields = new List<string>();
            var prs = type.GetProperties();
            foreach (var item in prs)
            {
                if (item.GetCustomAttribute<IgnoreUpdateAttribute>() == null)
                {
                    fields.Add(item.Name);
                }
            }

            return fields;
        }

        public Type GetTypeInList(Type listType)
        {
            if (this.IsList(listType))
            {
                Type type = listType.GetGenericArguments()[0];
                return type;
            }

            return null;
        }

        public bool IsList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public TDes MapData<TDes>(object origin)
        {
            return (TDes)this.MapData(origin, typeof(TDes));
        }

        public object MapData(object origin, Type desType)
        {
            if (origin == null || desType == null)
            {
                return null;
            }

            var des = Activator.CreateInstance(desType);
            var prsOrigin = origin.GetType().GetProperties();
            var prsDes = desType.GetProperties();

            foreach (var pro in prsOrigin)
            {
                var prd = prsDes.FirstOrDefault(n => n.Name == pro.Name);
                if (prd != null)
                {
                    prd.SetValue(des, pro.GetValue(origin));
                }
            }

            return des;
        }
    }
}
