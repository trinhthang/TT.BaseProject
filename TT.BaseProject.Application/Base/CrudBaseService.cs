using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Data;
using System.Reflection;
using TT.BaseProject.Application.Contracts.Base;
using TT.BaseProject.Application.Contracts.Common;
using TT.BaseProject.Application.Contracts.Crud;
using TT.BaseProject.Application.Exceptions;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Base;
using TT.BaseProject.Domain.Constant;
using TT.BaseProject.Domain.Context;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Application.Base
{
    public abstract class CrudBaseService<TRepo, TKey, TEntity, TEntityDtoEdit> : BaseService<TEntity, TRepo>, ICrudBaseService<TKey, TEntity, TEntityDtoEdit>
        where TEntityDtoEdit : TEntity, IRecordState
        where TRepo : IBusinessBaseRepo
    {
        #region Khai báo các service để khởi tạo qua IServiceProvider

        protected readonly IContextService _contextService;
        protected readonly ITypeService _typeService;
        protected readonly ISerializerService _serializerService;

        #endregion

        protected readonly int MASTER_INDEX = -1;

        protected CrudBaseService(TRepo repo, IServiceProvider serviceProvider) : base(repo, serviceProvider)
        {
            _contextService = serviceProvider.GetRequiredService<IContextService>();
            _typeService = serviceProvider.GetRequiredService<ITypeService>();
            _serializerService = serviceProvider.GetRequiredService<ISerializerService>();
        }

        public virtual async Task<TEntityDtoEdit> GetNewAsync(string param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = _repo.GetOpenConnection();
                return await this.GetNewAsync(cnn, param);
            }
            finally
            {
                _repo.CloseConnection(cnn);
            }
        }

        public virtual async Task<TEntityDtoEdit> GetEditAsync(TKey id)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = _repo.GetOpenConnection();
                return await this.GetEditAsync(cnn, id);
            }
            finally
            {
                _repo.CloseConnection(cnn);
            }
        }

        public virtual async Task<TEntityDtoEdit> GetDuplicateAsync(TKey id)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = _repo.GetOpenConnection();
                return await this.GetDuplicateAsync(cnn, id);
            }
            finally
            {
                _repo.CloseConnection(cnn);
            }
        }

        public virtual async Task<TEntityDtoEdit> SaveAsync(SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {

            IDbConnection cnn = null;
            try
            {
                cnn = _repo.GetOpenConnection();
                return await this.SaveAsync(cnn, parameter);
            }
            finally
            {
                _repo.CloseConnection(cnn);
            }
        }

        public virtual async Task<List<DeleteError>> DeleteAsync(DeleteParameter<TKey, TEntity> parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            var result = new List<DeleteError>();
            IDbConnection cnn = null;
            try
            {
                cnn = _repo.GetOpenConnection();

                for (int i = 0; i < parameter.Models.Count; i++)
                {
                    var item = parameter.Models[i];
                    try
                    {
                        await this.DeleteAsync(cnn, parameter, item);
                    }
                    catch (BusinessException ex)
                    {
                        result.Add(new DeleteError
                        {
                            Index = i,
                            Code = ex.ErrorCode ?? "Business",
                            Data = ex.ErrorData ?? ex.ErrorMessage
                        });
                    }
                    catch (Exception ex)
                    {
                        var msg = "Exception";
                        msg += "|" + ex.ToString();

                        result.Add(new DeleteError
                        {
                            Index = i,
                            Code = "Exception",
                            Data = msg
                        });
                    }
                }
            }
            finally
            {
                _repo.CloseConnection(cnn);
            }

            return result;
        }

        public virtual async Task<IList> GetListAsync(string sort)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IList> GetTreeAsync(string columns, string sort)
        {
            var data = await _repo.GetTreeAsync<TEntity>(columns, sort);
            return data;
        }

        #region Mothod

        protected virtual async Task<TEntityDtoEdit> GetNewAsync(IDbConnection cnn, string param)
        {
            var entity = this.CreateEditModel();

            //Khởi tạo các chi tiết - chỉ khởi tạo mảng rỗng
            var details = this.GetDetailAttribute();
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    this.GetDefaultRefData(entity, detail.Key);
                }
            }

            //TODO
            //Set Auto ID

            return entity;
        }

        public virtual TEntityDtoEdit CreateEditModel()
        {
            var model = Activator.CreateInstance<TEntityDtoEdit>();
            var keyField = _typeService.GetKeyField(typeof(TEntityDtoEdit));
            var masterId = keyField.GetValue(model);

            if (masterId is Guid || masterId is Guid?)
            {
                keyField.SetValue(model, Guid.NewGuid());
            }

            return model;
        }

        protected virtual Dictionary<PropertyInfo, DetailAttribute> GetDetailAttribute()
        {
            return _typeService.GetPropertys<DetailAttribute>(typeof(TEntityDtoEdit));
        }

        /// <summary>
        /// Khởi tạo dữ liệu mặc định cho detail
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="detailInfo"></param>
        /// <returns></returns>
        protected virtual IList GetDefaultRefData(TEntityDtoEdit entity, PropertyInfo detailInfo)
        {
            return null;
        }

        protected virtual async Task<TEntityDtoEdit> GetEditAsync(IDbConnection cnn, TKey id)
        {
            //master
            var model = await this.GetEditMasterAsync(cnn, id);
            if (model != null)
            {
                //xử lý edit version
                this.ProcessEditVersion(model);

                //load chi tiết
                await this.GetEditDetailAsync(cnn, model);

                //load link - chưa cần nghiệp vụ này
                //await this.GetEditLinkAsync(cnn, model);
            }

            //validate sau khi lấy dữ liệu
            await this.ValidateAfterEditAsync(cnn, model);

            return model;
        }

        protected virtual async Task<TEntityDtoEdit> GetEditMasterAsync(IDbConnection cnn, object id)
        {
            var model = await _repo.GetByIdAsync<TEntityDtoEdit>(cnn, typeof(TEntityDtoEdit), id);
            return model;
        }

        protected virtual void ProcessEditVersion(object model)
        {
            if (model is IRecordVersion)
            {
                var versionAttr = _typeService.GetPropertys<EditVersionAttribute>(model.GetType()).FirstOrDefault();
                var version = this.GenerateVersion(model);
                if (version != null)
                {
                    versionAttr.Key.SetValue(model, version);
                }
            }
        }

        protected virtual long? GenerateVersion(object entity)
        {
            var modelType = typeof(TEntityDtoEdit);
            var versionAttr = _typeService.GetPropertys<EditVersionAttribute>(modelType).FirstOrDefault();
            if (versionAttr.Key != null)
            {
                var attr = (EditVersionAttribute)versionAttr.Value;
                var pr = entity.GetType().GetProperty(attr.DataField);
                if (pr != null)
                {
                    var value = pr.GetValue(entity);
                    if (value != null)
                    {
                        if (value is DateTime)
                        {
                            return ((DateTime)value).Ticks;
                        }
                        else if (value is long)
                        {
                            return (long)value;
                        }
                        else if (value is int)
                        {
                            return Convert.ToInt64(value);
                        }
                    }
                }
            }

            return null;
        }

        protected virtual async Task GetEditDetailAsync(IDbConnection cnn, TEntityDtoEdit master)
        {
            var config = this.GetDetailAttribute();
            if (config != null && config.Count > 0)
            {
                var keyField = _typeService.GetKeyField(typeof(TEntityDtoEdit));
                var masterId = keyField.GetValue(master);

                foreach (var item in config)
                {
                    var detail = await this.LoadRefByMasterAsync(cnn, item.Key, item.Value, masterId);
                    item.Key.SetValue(master, detail);
                }
            }
        }

        protected virtual async Task<IList> LoadRefByMasterAsync(IDbConnection cnn, PropertyInfo info, IMasterRefAttribute attr, object masterId)
        {
            var type = _typeService.GetTypeInList(info.PropertyType);
            var data = await _repo.GetRefByMasterAsync(cnn, attr, type, masterId);
            return data;
        }

        public virtual async Task ValidateAfterEditAsync(IDbConnection cnn, TEntityDtoEdit model)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task<TEntityDtoEdit> GetDuplicateAsync(IDbConnection cnn, TKey id)
        {
            var model = await this.GetEditAsync(cnn, id);

            if (model != null)
            {
                model.state = ModelState.Duplicate;

                //sinh khóa chính mới
                this.ProcessNewId(id, model);

                //TODO Set Auto ID 

                //Cập nhật dữ liệu khi nhân bản
                this.ProcessDuplicateDetailData(model);
            }

            return model;
        }

        protected virtual void ProcessNewId(TKey id, TEntityDtoEdit model)
        {
            TKey masterId = default(TKey);
            if (id is int || id is long)
            {
                masterId = (TKey)(object)0;
            }
            else if (id is Guid)
            {
                masterId = (TKey)(object)Guid.NewGuid();
            }

            //cập nhật master
            var keyField = _typeService.GetKeyField(typeof(TEntity));
            keyField.SetValue(model, masterId);
        }

        protected virtual void ProcessDuplicateDetailData(TEntityDtoEdit model)
        {
            var details = this.GetDetailAttribute();
            //Nếu không có details thì break khỏi hàm
            if (details == null || details.Count == 0)
            {
                return;
            }

            var keyField = _typeService.GetKeyField(typeof(TEntityDtoEdit));
            var masterId = keyField.GetValue(model);

            foreach (var detail in details)
            {
                var detailData = detail.Key.GetValue(model);
                if (detailData == null)
                {
                    continue;
                }
                if (!(detailData is IList listDetail))
                {
                    continue;
                }
                if (listDetail.Count == 0)
                {
                    continue;
                }

                var masterKeyField = detail.Value.MasterKeyField;

                this.ProcessDuplicateListDetailData(listDetail, masterKeyField, masterId);
            }
        }

        private void ProcessDuplicateListDetailData(IList listDetail, string masterKeyField, object masterId)
        {
            var detailItemType = listDetail[0].GetType();
            var detailKeyField = _typeService.GetKeyField(detailItemType);
            var foreignKeyField = detailItemType.GetProperty(masterKeyField);
            foreach (var item in listDetail)
            {
                //Cập nhật PK
                if (detailKeyField != null)
                {
                    this.ProcessNewKey(item, detailKeyField, true);
                }

                //Cập nhật khóa ngoại
                if (foreignKeyField != null)
                {
                    foreignKeyField.SetValue(item, masterId);
                }

                if (item is IRecordState)
                {
                    ((IRecordState)item).state = ModelState.Insert;
                }
            }
        }

        /// <summary>
        /// Xử lý sinh khóa chính cho model nếu chưa có
        /// Chỉ áp dụng cho khóa chính dạng guild và đang có giá trị là null hoặc Guid.Empty
        /// </summary>
        /// <param name="model"></param>
        /// <param name="keyField"></param>
        /// <param name="force"></param>
        /// <returns>Giá trị khóa chính của bản ghi</returns>
        private object ProcessNewKey(object model, PropertyInfo keyField, bool force = false)
        {
            var value = keyField.GetValue(model);
            if ((keyField.PropertyType == typeof(Guid) || keyField.PropertyType == typeof(Guid?))
                && (force || value == null || ((Guid)value) == Guid.Empty))
            {
                value = Guid.NewGuid();
                keyField.SetValue(model, value);
            }

            return value;
        }

        public virtual async Task<TEntityDtoEdit> SaveAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            await this.BeforeSaveAsync(cnn, parameter);

            await this.ValidateSaveAsync(cnn, parameter);

            using var transaction = cnn.BeginTransaction();
            try
            {
                await this.SaveDataAsync(transaction, parameter);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            await this.AfterSaveAsync(cnn, parameter);

            if (parameter.ReturnRecord)
            {
                var data = await this.GetReturnRecordAsync(cnn, parameter.Model);
                return data;
            }

            return default(TEntityDtoEdit);
        }

        protected virtual async Task BeforeSaveAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            var master = parameter.Model;
            var detailConfig = this.GetDetailAttribute();
            object masterId = null;
            var keyField = _typeService.GetKeyField(typeof(TEntityDtoEdit));

            switch (master.state)
            {
                case ModelState.Insert:
                    masterId = this.ProcessNewKey(master, keyField);
                    break;
                case ModelState.Update:
                    masterId = keyField.GetValue(master);
                    parameter.Old = await this.GetDtoEditAsync(cnn, (TKey)masterId);
                    break;
            }

            parameter.Id = masterId;

            //update history
            this.ProcessHistoryInfo(master);

            //detail
            if (detailConfig != null && detailConfig.Count > 0)
            {
                this.UpdateRefBeforeSave(detailConfig, masterId, master);
            }

            //link
        }

        public virtual void ProcessHistoryInfo(object data)
        {
            if (data is IRecordState)
            {
                var contextData = _contextService.Get();
                var now = DateTime.Now;
                switch (((IRecordState)data).state)
                {
                    case ModelState.Insert:
                        if (data is IRecordCreate)
                        {
                            ((IRecordCreate)data).created_date = now;
                            ((IRecordCreate)data).create_by = contextData.UserName;
                        }
                        break;
                    case ModelState.Update:
                        if (data is IRecordModify)
                        {
                            ((IRecordModify)data).modified_date = now;
                            ((IRecordModify)data).modified_by = contextData.UserName;
                        }
                        break;
                }
            }
        }

        protected virtual async Task<TEntityDtoEdit> GetDtoEditAsync(IDbConnection cnn, TKey id)
        {
            var dtoEdit = await GetDataMasterDetailAsync(cnn, id);
            return dtoEdit;
        }

        protected virtual async Task<TEntityDtoEdit> GetDataMasterDetailAsync(IDbConnection cnn, TKey id)
        {
            //master
            var model = await this.GetEditMasterAsync(cnn, id);
            if (model != null)
            {
                //load detail
                await this.GetEditDetailAsync(cnn, model);
            }

            return model;
        }

        private void UpdateRefBeforeSave<TAttribute>(Dictionary<PropertyInfo, TAttribute> configs, object masterKey, object master, bool hasDetail = false)
        where TAttribute : IMasterRefAttribute
        {
            foreach (var config in configs)
            {
                var datas = (IList)config.Key.GetValue(master);
                if (datas == null || datas.Count == 0)
                {
                    continue;
                }
                var type = datas[0].GetType();
                var frField = type.GetProperty(config.Value.MasterKeyField);
                var pkField = _typeService.GetKeyField(type);

                if (frField == null)
                {
                    throw new MissingFieldException($"Thiếu cấu hình khóa ngoại {config.Value.Type.Name} với {master.GetType().Name}");
                }

                if (pkField == null)
                {
                    throw new MissingFieldException($"Thiếu cấu hình khóa chính {config.Value.Type.Name}");
                }

                Dictionary<PropertyInfo, DetailAttribute> detailConfig = null;
                if (hasDetail)
                {
                    detailConfig = _typeService.GetPropertys<DetailAttribute>(type);
                }

                foreach (IRecordState data in datas)
                {
                    this.UpdateRefDetailBeforeSave(data, detailConfig, masterKey, pkField, frField);
                }
            }
        }

        private void UpdateRefDetailBeforeSave(IRecordState data, Dictionary<PropertyInfo, DetailAttribute> detailConfig, object masterKey, PropertyInfo pkField, PropertyInfo frField)
        {
            var key = this.ProcessNewKey(data, pkField);
            if (data.state == ModelState.Insert)
            {
                frField.SetValue(data, masterKey);
            }

            if (detailConfig != null && detailConfig.Count > 0)
            {
                this.UpdateRefBeforeSave(detailConfig, key, data);
            }
        }

        public virtual async Task ValidateSaveAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            //validate require
            await this.ValidateSaveRequireAsync(cnn, parameter);

            //mode edit => check edit version
            if (parameter.Model.state == ModelState.Update && parameter.Model is IRecordVersion)
            {
                await ValidateSaveVersionAsync(cnn, parameter);
            }

            //check duplicate code
            await ValidateSaveDuplicateAsync(cnn, parameter);
        }

        public virtual async Task<ValidateResult> ValidateSaveRequireAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            var dic = new Dictionary<string, List<ValidateRequiredData>>();

            //validate master
            this.ValidateMasterWhenSave(ref dic, parameter.Model);

            //validate detail
            this.ValidateDetailWhenSave(ref dic, parameter.Model);

            if (dic.Count > 0)
            {
                throw new BusinessException()
                {
                    ErrorCode = ErrorCodes.Validate,
                    ErrorData = new ValidateResult
                    {
                        Type = ValidateResultType.Error,
                        Code = ValidateCode.Required.ToString(),
                        Data = dic
                    }
                };
            }

            return await Task.FromResult<ValidateResult>(null);
        }

        private void ValidateMasterWhenSave(ref Dictionary<string, List<ValidateRequiredData>> validateRequiredDatas, TEntityDtoEdit model)
        {
            var masterType = typeof(TEntity);
            var prs = _typeService.GetPropertys<RequiredAttribute>(typeof(TEntity));
            if (prs != null && prs.Count > 0)
            {
                var table = _typeService.GetTableName(masterType);
                var fields = this.ValidateRequired(table, MASTER_INDEX, model, prs.Select(n => n.Key));
                if (fields.Count > 0)
                {
                    validateRequiredDatas.Add(table, new List<ValidateRequiredData>
                    {
                        new ValidateRequiredData
                        {
                            Index = MASTER_INDEX,
                            Fields = fields
                        }
                    });
                }
            }
        }

        private void ValidateDetailWhenSave(ref Dictionary<string, List<ValidateRequiredData>> validateRequiredDatas, TEntityDtoEdit model)
        {
            var details = this.GetDetailAttribute();
            if (details == null || details.Count == 0)
            {
                return;
            }

            foreach (var detail in details)
            {
                var detailData = (IList)detail.Key.GetValue(model);
                if (detailData != null || detailData.Count == 0)
                {
                    continue;
                }
                var detailType = detailData[0].GetType();
                var detailRes = new List<ValidateRequiredData>();
                var prsDetail = _typeService.GetPropertys<RequiredAttribute>(detailType);
                var table = _typeService.GetTableName(detail.Value.Type);

                for (int i = 0; i < detailData.Count; i++)
                {
                    var item = detailData[i];
                    var fields = this.ValidateRequired(table, i, item, prsDetail.Select(n => n.Key));
                    if (fields.Count == 0)
                    {
                        continue;
                    }
                    detailRes.Add(new ValidateRequiredData
                    {
                        Index = i,
                        Fields = fields
                    });
                }

                if (detailRes.Count > 0)
                {
                    validateRequiredDatas[table] = detailRes;
                }
            }
        }

        public virtual List<string> ValidateRequired(string name, int index, object record, IEnumerable<PropertyInfo> prs)
        {
            var fields = new List<string>();
            if (prs != null && prs.Count() > 0)
            {
                foreach (var pr in prs)
                {
                    object value = pr.GetValue(record);
                    if (value == null
                        || (value is Guid && Guid.Empty.Equals(value))
                        || (value is string && string.Empty.Equals(value))
                        || ((value is decimal || value is float || value is int || value is long) && 0.Equals(value))
                        )
                    {
                        fields.Add(pr.Name);
                    }
                }
            }

            return fields;
        }

        public virtual async Task ValidateSaveVersionAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            var ruleCode = ValidateCode.EditVersion.ToString();
            if (!parameter.CheckIgnore(ruleCode))
            {
                if (!this.HasObsolate((IRecordVersion)parameter.Model, parameter.Old))
                {
                    await Task.CompletedTask;
                    return;
                }
                throw new BusinessException()
                {
                    ErrorCode = ErrorCodes.Validate,
                    ErrorData = new ValidateResult
                    {
                        Type = ValidateResultType.Error,
                        Code = ruleCode
                    }
                };
            }
        }

        protected virtual bool HasObsolate(IRecordVersion model, TEntity entity)
        {
            var version = this.GenerateVersion(entity);
            var entityVersion = model.RecordVersion;
            if (version.HasValue && version > entityVersion)
            {
                return true;
            }

            return false;
        }

        public virtual async Task ValidateSaveDuplicateAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            var ruleCode = ValidateCode.Duplicate.ToString();
            if (parameter.CheckIgnore(ruleCode))
            {
                return;
            }
            var type = typeof(TEntity);
            var model = parameter.Model;
            var uniqueFields = _typeService.GetPropertys<UniqueAttribute>(type);

            if (uniqueFields != null || uniqueFields.Count == 0)
            {
                return;
            }

            List<PropertyInfo> keyFields = null;
            if (model.state == ModelState.Update)
            {
                var temp = _typeService.GetPropertys<KeyAttribute>(type);
                if (temp == null || temp.Count == 0)
                {
                    return;
                }
                keyFields = temp.Select(n => n.Key).ToList();
            }

            var uniqueProps = uniqueFields.Select(n => n.Key).ToList();
            var dup = await _repo.HasDuplicateAsync(cnn, parameter.Model, keyFields, uniqueProps);
        }

        public virtual async Task SaveDataAsync(IDbTransaction transaction, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            //master
            await this.SubmitItemAsync(transaction, typeof(TEntity), parameter.Model);

            //detal
            await this.SaveDataDetailAsync(transaction, parameter);

            //submit link
        }

        protected virtual async Task SubmitItemAsync(IDbTransaction transaction, Type type, IRecordState model)
        {
            switch (model.state)
            {
                case ModelState.Insert:
                    await _repo.InsertAsync(transaction, type, model);
                    break;
                case ModelState.Update:
                    await _repo.UpdateAsync(transaction, type, model);
                    break;
                case ModelState.Delete:
                    await _repo.DeleteAsync(transaction, type, model);
                    break;
            }
        }
        protected virtual async Task SaveDataDetailAsync(IDbTransaction transaction, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            var items = this.GetDetailAttribute();
            if (items != null && items.Count > 0)
            {
                var masterId = parameter.Id;

                foreach (var item in items)
                {
                    var detailData = (IList)item.Key.GetValue(parameter.Model);

                    var saveData = await this.ProcessDetailItemBeforeSaveAsync(transaction, item.Value, detailData, masterId);

                    await this.UpdateDetailDataAsync(transaction, item.Value, saveData);
                }
            }
        }

        protected virtual async Task<IList> ProcessDetailItemBeforeSaveAsync(IDbTransaction transaction, DetailAttribute attr, IList data, object masterKey)
        {
            return await this.ProcessRefInfoBeforeSaveAsync(transaction, attr, data, masterKey);
        }

        protected virtual async Task<IList> ProcessRefInfoBeforeSaveAsync(IDbTransaction transaction, DetailAttribute attr, IList data, object masterKey)
        {
            if (data != null && data.Count > 0
                && (
                masterKey is int
                || masterKey is uint
                || masterKey is long
                || masterKey is ulong
                )
                )
            {
                var firstItem = data[0];
                var type = firstItem.GetType();
                var pr = type.GetProperty(attr.MasterKeyField);
                foreach (var item in data)
                {
                    await this.ProcessRefInfoItemBeforeSaveAsync(transaction, masterKey, item, pr);
                }
            }

            return data;
        }

        protected virtual async Task ProcessRefInfoItemBeforeSaveAsync(IDbTransaction transaction, object masterId, object data, PropertyInfo frKey)
        {
            if (!(data is IRecordState) || (data as IRecordState).state == ModelState.Insert)
            {
                frKey.SetValue(data, masterId);
            }

            this.ProcessHistoryInfo(data);
            await Task.CompletedTask;
        }

        protected virtual async Task UpdateDetailDataAsync(IDbTransaction transaction, DetailAttribute detailAttribute, IList detailData)
        {
            if (detailData != null && detailData.Count > 0)
            {
                foreach (IRecordState item in detailData)
                {
                    await this.SubmitItemAsync(transaction, detailAttribute.Type, item);
                }
            }
        }

        protected virtual async Task AfterSaveAsync(IDbConnection cnn, SaveParameter<TEntityDtoEdit, TEntity> parameter)
        {
            //TODO auditing log

            //TODO autoID
        }

        /// <summary>
        /// Lấy bản ghi trả về client sau khi Cất
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<TEntityDtoEdit> GetReturnRecordAsync(IDbConnection cnn, TEntityDtoEdit model)
        {
            var keyField = _typeService.GetKeyField(typeof(TEntityDtoEdit));
            var masterId = keyField.GetValue(model);
            var data = await this.GetEditAsync(cnn, (TKey)masterId);

            return data;
        }

        public virtual async Task DeleteAsync(IDbConnection cnn, DeleteParameter<TKey, TEntity> parameter, TEntity model)
        {
            await this.BeforeDeleteAsync(cnn, parameter, model);

            await this.ValidateDeleteAsync(cnn, parameter, model);

            await this.DeleteDataAsync(cnn, parameter, model);

            await this.AfterDeleteAsync(cnn, parameter, model);
        }

        public virtual async Task BeforeDeleteAsync(IDbConnection cnn, DeleteParameter<TKey, TEntity> parameter, TEntity model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task ValidateDeleteAsync(IDbConnection cnn, DeleteParameter<TKey, TEntity> parameter, TEntity model)
        {
            await Task.CompletedTask;
        }

        public virtual async Task DeleteDataAsync(IDbConnection cnn, DeleteParameter<TKey, TEntity> parameter, TEntity model)
        {
            using var transaction = cnn.BeginTransaction();
            try
            {
                await this.BeforeDeleteDataAsync(transaction, parameter);

                await _repo.DeleteAsync(transaction, model);

                await this.AfterDeleteDataAsync(transaction, parameter);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public virtual async Task AfterDeleteAsync(IDbConnection cnn, DeleteParameter<TKey, TEntity> parameter, TEntity model)
        {
            //TODO AuditingLog

            await Task.CompletedTask;
        }

        public virtual async Task BeforeDeleteDataAsync(IDbTransaction transaction, DeleteParameter<TKey, TEntity> parameter)
        {
            await Task.CompletedTask;
        }

        public virtual async Task AfterDeleteDataAsync(IDbTransaction transaction, DeleteParameter<TKey, TEntity> parameter)
        {
            await Task.CompletedTask;
        }
        #endregion
    }
}
