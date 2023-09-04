using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Crud
{
    public class PagingResult
    {
        public IList PageData { get; set; }

        public bool Empty { get; set; }
    }

    public class PagingSummaryResult
    {
        public long Total { get; set; }

        public object SummaryData { get; set; }
    }
}
