using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Context
{
    public interface IContextService
    {
        void Set(ContextData contextData);

        ContextData Get();
    }
}
