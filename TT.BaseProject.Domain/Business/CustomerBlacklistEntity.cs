﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TT.BaseProject.Domain.Attributes;
using TT.BaseProject.Domain.Entity;
using TT.BaseProject.Domain.Enum;

namespace TT.BaseProject.Domain.Business
{
    // <summary>
    /// Danh sách đen
    /// </summary>
    [Table("dic_customer_blacklist")]
    public class CustomerBlacklistEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid customer_blacklist_id { get; set; }

        public Guid customer_id { get; set; }

        public int customer_blacklist_status { get; set; } = 0;

        /// <summary>
        /// ID người dùng tạo
        /// </summary>
        public Guid user_id { get; set; }

        public string description { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
