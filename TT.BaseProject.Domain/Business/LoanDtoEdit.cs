using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    public class LoanDtoEdit : LoanEntity, IRecordState, IRecordVersion
    {
        public ModelState state { get; set; }

        public long RecordVersion { get; set; }
    }
}
