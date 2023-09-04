using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Application.Contracts.Base;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Domain.Base;
using TT.BaseProject.Domain.Crud;

namespace TT.BaseProject.Application.Base
{
    public abstract class BaseService<TEntity, TRepo> : IBaseService<TEntity>
        where TRepo : IBaseRepo
    {
        protected TRepo _repo;
        protected ITypeService _typeService;

        public BaseService(TRepo repo, IServiceProvider serviceProvider)
        {
            _repo = repo;
            _typeService = serviceProvider.GetRequiredService<ITypeService>();
        }

        public virtual async Task<IList> GetDataById(string columns, string ids)
        {
            if (string.IsNullOrEmpty(columns) || string.IsNullOrEmpty(ids))
            {
                return default(IList);
            }

            var key = _typeService.GetKey(typeof(TEntity));

            if (key == null)
            {
                return default(IList);
            }

            return await _repo.GetDynamicAsync(typeof(TEntity), columns, key.Name, ids.Split(','), "IN");
        }

        public virtual async Task<PagingResult> GetPagingAsync(string sort, int skip, int take, string columns, string filter, string emptyFilter)
        {
            var data = await _repo.GetPagingAsync(typeof(TEntity), sort, skip, take, columns, filter, emptyFilter);
            return data;
        }

        public virtual async Task<PagingSummaryResult> GetPagingSummaryAsync(string columns, string filter)
        {
            var data = await _repo.GetPagingSummaryAsync(typeof(TEntity), columns, filter);
            return data;
        }

        public virtual async Task<IList> GetComboboxPagingAsync(string sort, int skip, int take, string columns, string filter, string selectedItem)
        {
            return await _repo.GetComboboxPagingAsync(typeof(TEntity), sort, skip, take, columns, filter, selectedItem);
        }
    }
}
