using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace eShopSolution.Data.Enums
{
    public enum OrderStatus
    {
        InProgress,
        Confirmed,
        Shipping,
        Success,
        Canceled
    }
}
