using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TT.BaseProject.Domain.Crud
{
    public class PagingParameter
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Sort { get; set; }
        public string Filter { get; set; }
        public string EmptyFilter { get; set; }
        public string Columns { get; set; }
        public string SelectedItem { get; set; }
        public PagingDataType type { get; set; }
    }

    public class PagingSummaryParameter
    {
        public string condition { get; set; }
        public string filter { get; set; }
        public string columns { get; set; }
    }

    public enum PagingDataType
    {
        Data = 0,

        Summary = 1
    }
}
