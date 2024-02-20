using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    [Table("company")]
    public class CompanyEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid company_id { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
