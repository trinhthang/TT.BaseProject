using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Attributes
{
    public class TableAttribute : Attribute
    {
        public string Table { get; set; }

        public TableAttribute(string table)
        {
            this.Table = table;
        }
    }
}
