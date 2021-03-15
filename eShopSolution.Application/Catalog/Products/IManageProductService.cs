using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopSolution.Application.Catalog.Products.Dtos;
using eShopSolution.Application.Catalog.Products.Dtos.Manage;
using eShopSolution.Application.Dtos;
using eShopSolution.Data.EF;
using eShopSolution.Data.Entities;
using eShopSolution.Utilities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace eShopSolution.Application.Catalog.Products
{
    public interface IManageProductService
    {
        Task<int> Create(ProductCreateRequest request);

        Task<int> Update(ProductUpdateRequest request);

        Task<int> Delete(int productId);

        Task<bool> UpdatePrice(int productId, decimal newPrice);

        Task<bool> UpdateStock(int productId, int addedQuantity);

        Task AddViewCount(int productId);

        Task<List<ProductViewModel>> GetAll();

        Task<PagedResult<ProductViewModel>> GetAllPaging(GetProductPagingRequest request);

    }

    public class ManageProductService : IManageProductService
    {
        private readonly EShopDbContext _context;
        public ManageProductService(EShopDbContext context)
        {
            _context = context;
        }
        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new Product
            {
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                ProductTranslations = new List<ProductTranslation>
                {
                    new ProductTranslation
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Details = request.Details,
                        SeoDescription = request.SeoDescription,
                        SeoAlias = request.SeoAlias,
                        SeoTitle = request.SeoTitle,
                        LanguageId = request.LanguageId
                    }
                }
            };
            await _context.Products.AddAsync(product);
            return await _context.SaveChangesAsync();
        }

        public Task<int> Update(ProductUpdateRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product: {productId}");
            _context.Products.Remove(product);
            return await _context.SaveChangesAsync();
        }

        public Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UpdateStock(int productId, int addedQuantity)
        {
            throw new System.NotImplementedException();
        }

        public async Task AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount += 1; 
            await _context.SaveChangesAsync();

        }

        public Task<List<ProductViewModel>> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetProductPagingRequest request)
        {
            var query = from p in _context.Products
                join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                join c in _context.Categories on pic.CategoryId equals c.Id
                where pt.Name.Contains(request.Keyword)
                select new{ p, pt, pic};

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.pt.Name.Contains(request.Keyword));
            }

            if (request.CategoryIds.Count > 0)
            {
                query = query.Where(p => request.CategoryIds.Contains(p.pic.CategoryId));
            }

            var totalRow = await query.CountAsync();

            var data = await query
                                        .Skip((request.PageIndex - 1) * request.PageSize)
                                        .Take(request.PageSize)
                                        .Select(x => new ProductViewModel
                                        {
                                            Id = x.p.Id,
                                            Name = x.pt.Name,
                                            DateCreated = x.p.DateCreated,
                                            Description = x.pt.Description,
                                            Details = x.pt.Details,
                                            LanguageId = x.pt.LanguageId,
                                            OriginalPrice = x.p.OriginalPrice,
                                            Price = x.p.Price,
                                            SeoAlias = x.pt.SeoAlias,
                                            SeoDescription = x.pt.SeoDescription,
                                            SeoTitle = x.pt.SeoTitle,
                                            Stock = x.p.Stock,
                                            ViewCount = x.p.ViewCount
                                        }).ToListAsync();

            var pagedResult = new PagedResult<ProductViewModel>
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pagedResult;
        }
    }
}