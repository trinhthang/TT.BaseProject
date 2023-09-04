using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    public class DeleteParameter<TKey, TEntity> : CrudParameter
    {
        public List<TEntity> Models { get; set; }

        public IList Ids { get; set; }
    }
}
