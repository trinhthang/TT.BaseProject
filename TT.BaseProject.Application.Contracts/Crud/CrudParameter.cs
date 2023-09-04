using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    public class CrudParameter
    {
        public List<String> Ignores { get; set; }

        public virtual bool CheckIgnore(string code)
        {
            if (this.Ignores != null && !string.IsNullOrEmpty(code))
            {
                foreach (var item in this.Ignores)
                {
                    if (code.Equals(item, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
