using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Attributes
{
    public class MasterTableAttribute : Attribute
    {
        public string Name { get; set; }

        public MasterTableAttribute(string name)
        {
            this.Name = name;
        }
    }
}
