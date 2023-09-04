using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Context;

namespace TT.BaseProject.HostBase.Service
{
    public class ContextService : IContextService
    {
        private ContextData _contextData = null;

        public ContextService()
        {

        }

        public ContextData Get()
        {
            return _contextData;
        }

        public void Set(ContextData contextData)
        {
            _contextData = contextData;
        }
    }
}
