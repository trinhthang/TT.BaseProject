using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Attributes
{
    public class DetailAttribute : Attribute, IMasterRefAttribute
    {
        /// <summary>
        /// Tên trường mapping với master key
        /// </summary>
        public string MasterKeyField { get; set; }

        /// <summary>
        /// Kiểu dữ liệu
        /// </summary>
        public Type Type { get; set; }

        public DetailAttribute(string masterKeyField, Type type)
        {
            this.MasterKeyField = masterKeyField;
            this.Type = type;
        }
    }
}
