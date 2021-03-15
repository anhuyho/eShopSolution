using System.Collections.Generic;

namespace eShopSolution.Application.Dtos
{
    public class PagedResult<T>
    {
        public int TotalRecord { get; set; }
        public List<T> Items { get; set; }
    }
}
