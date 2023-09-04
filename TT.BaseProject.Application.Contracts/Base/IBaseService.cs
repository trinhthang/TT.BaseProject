using System.Collections;
using TT.BaseProject.Domain.Crud;

namespace TT.BaseProject.Application.Contracts.Base
{
    public interface IBaseService<TEntity>
    {
        Task<PagingResult> GetPagingAsync(string sort, int skip, int take, string columns, string filter, string emptyFilter);

        Task<PagingSummaryResult> GetPagingSummaryAsync(string columns, string filter);

        Task<IList> GetDataById(string columns, string ids);

        Task<IList> GetComboboxPagingAsync(string sort, int skip, int take, string columns, string filter, string selectedItem);
    }
}
