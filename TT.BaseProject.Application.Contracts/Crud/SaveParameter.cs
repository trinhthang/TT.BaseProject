using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TT.BaseProject.Application.Contracts.Crud
{
    public class SaveParameter<TEntityDtoEdit, TEntity> : CrudParameter
    {
        public TEntityDtoEdit Model { get; set; }

        public bool ReturnRecord { get; set; }

        [JsonIgnore]
        public object Id { get; set; }

        [JsonIgnore]
        public TEntityDtoEdit Old { get; set; }

    }
}
