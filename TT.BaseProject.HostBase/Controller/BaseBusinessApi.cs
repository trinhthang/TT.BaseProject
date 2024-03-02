using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TT.BaseProject.Application.Contracts.Base;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Application.Contracts.Crud;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.Crud;
using TT.BaseProject.Domain.Entity;

namespace TT.BaseProject.HostBase.Controller
{
    public abstract class BaseBusinessApi<TService, TKey, TEntity, TEntityDtoEdit> : BaseApi
         where TEntityDtoEdit : IRecordState, TEntity
         where TService : ICrudBaseService<TKey, TEntity, TEntityDtoEdit>
    {
        protected readonly TService _service;
        protected static readonly Type EntityType = typeof(TEntity);
        protected readonly IContextService _contextService;
        protected readonly ISerializerService _serializerService;

        public BaseBusinessApi(TService service, IServiceProvider serviceProvider)
        {
            this.ControllerName = "BusinessAPI";

            _service = service;
            _contextService = serviceProvider.GetRequiredService<IContextService>();
            _serializerService = serviceProvider.GetRequiredService<ISerializerService>();
        }

        #region business

        [HttpPost("paging")]
        public virtual async Task<IActionResult> GetPaging([FromBody] PagingParameter param)
        {
            switch (param.type)
            {
                case PagingDataType.Summary:
                    var summary = await _service.GetPagingSummaryAsync(param.Columns, param.Filter);
                    return Ok(summary);
                default:
                    var data = await _service.GetPagingAsync(param.Sort, param.Skip, param.Take, param.Columns, param.Filter, param.EmptyFilter);
                    return Ok(data);
            }
        }

        [HttpPost("fullList")]
        public virtual async Task<IActionResult> GetFull(string sort)
        {
            return Ok(await _service.GetListAsync(sort));
        }

        [HttpPost("new")]
        public virtual async Task<IActionResult> GetNew(string param)
        {
            var data = await _service.GetNewAsync(param);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetEdit(TKey id)
        {
            var data = await _service.GetEditAsync(id);

            if (data == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Not found record {id}");
            }

            return Ok(data);
        }

        [HttpPost("{ids}")]
        public virtual async Task<IActionResult> GetDataById(ReferencesParameter param)
        {
            var data = await _service.GetDataById(param.columns, param.ids);

            if (data == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Not found record");
            }

            return Ok(data);
        }

        [HttpGet("duplicate")]
        public virtual async Task<IActionResult> GetDuplicate(TKey id)
        {
            var data = await _service.GetDuplicateAsync(id);

            if (data == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Not found record {id}");
            }

            return Ok(data);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Insert([FromBody] SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {

            parameter.Model.state = Domain.Enum.ModelState.Insert;
            var result = await _service.SaveAsync(parameter);

            return Ok(result);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Update([FromBody] SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            parameter.Model.state = Domain.Enum.ModelState.Update;

            var result = await _service.SaveAsync(parameter);

            return Ok(result);
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete([FromBody] DeleteParameter<TKey, TEntity> parameter)
        {
            var result = await _service.DeleteAsync(parameter);
            return Ok(result);
        }

        [HttpPost("combobox")]
        public virtual async Task<IActionResult> GetComboboxPaging([FromBody] PagingParameter param)
        {
            var data = await _service.GetComboboxPagingAsync(param.Sort, param.Skip, param.Take, param.Columns, param.Filter, param.SelectedItem);
            return Ok(data);
        }

        #endregion
    }
}
