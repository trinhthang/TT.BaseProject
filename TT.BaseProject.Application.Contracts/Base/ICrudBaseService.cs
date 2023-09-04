using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Crud;
using TT.BaseProject.Domain.Entity;

namespace TT.BaseProject.Application.Contracts.Base
{
    public interface ICrudBaseService<TKey, TEntity, TEntityDtoEdit> : IBaseService<TEntity>
        where TEntityDtoEdit : TEntity, IRecordState
    {
        Task<TEntityDtoEdit> GetNewAsync(string param);
        Task<TEntityDtoEdit> GetEditAsync(TKey id);
        Task<TEntityDtoEdit> GetDuplicateAsync(TKey id);
        Task<TEntityDtoEdit> SaveAsync(SaveParameter<TEntityDtoEdit, TEntity> parameter);
        Task<List<DeleteError>> DeleteAsync(DeleteParameter<TKey, TEntity> parameter);
        Task<IList> GetListAsync(string sort);
        Task<IList> GetTreeAsync(string columns, string sort);
    }
}
