using System.Collections.Generic;

namespace eShopSolution.ViewModels.Common
{
    public class PagedResult<T>
    {
        public int TotalRecord { get; set; }
        public List<T> Items { get; set; }
    }
}
