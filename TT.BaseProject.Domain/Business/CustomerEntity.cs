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
    /// Khách hàng
    /// </summary>
    [Table("dic_customer")]
    public class CustomerEntity : IRecordCreate, IRecordModify
    {
        [Key]
        public Guid customer_id { get; set; }

        public string customer_name { get; set; }

        public string identity_number { get; set; }

        public string phone_number { get; set; }

        public string description { get; set; }

        /// <summary>
        /// ID người dùng tạo
        /// </summary>
        public Guid user_id { get; set; }

        public DateTime? created_date { get; set; }

        public string create_by { get; set; }

        public DateTime? modified_date { get; set; }

        public string modified_by { get; set; }
    }
}
