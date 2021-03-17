using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using eShopSolution.Application.Common;
using eShopSolution.Data.EF;
using eShopSolution.Data.Entities;
using eShopSolution.Utilities.Exceptions;
using eShopSolution.ViewModels.Catalog.Products;
using eShopSolution.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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

        Task<PagedResult<ProductViewModel>> GetAllPaging(GetManageProductPagingRequest request);

        Task<int> AddImages(int productId, List<IFormFile> files);

        Task<int> RemoveImage(int imageId);

        Task<int> UpdateImage(int imageId, string caption, bool isDefault);


        Task<List<ProductImageViewModel>> GetListImages(int productId);
    }

    public class ManageProductService : IManageProductService
    {
        private readonly EShopDbContext _context;
        private readonly IStorageService _storageService;
        public ManageProductService(EShopDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
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

            if (request.ThumbnailImage != null)
            {
                product.ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Caption = "Thumbnail",
                        DateCreated = DateTime.Now,
                        FileSize = request.ThumbnailImage.Length,
                        ImagePath = await SaveFile(request.ThumbnailImage),
                        IsDefault = true,
                        SortOrder = 1
                    }
                };
            }

            await _context.Products.AddAsync(product);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            var productTranslation = await _context.ProductTranslations
                .FirstOrDefaultAsync(x => x.ProductId == request.Id
                                     && x.LanguageId == request.LanguageId);

            if (product == null || productTranslation == null) 
                throw new EShopException($"Cannot find a product with id: {request.Id}");

            productTranslation.Name = request.Name;
            productTranslation.SeoAlias = request.SeoAlias;
            productTranslation.SeoDescription = request.SeoDescription;
            productTranslation.SeoTitle = request.SeoTitle;
            productTranslation.Description = request.Description;
            productTranslation.Details = request.Details;


            if (request.ThumbnailImage != null)
            {
                var thumbnailImage =
                    await _context.ProductImages
                        .FirstOrDefaultAsync(i => i.IsDefault == true 
                                                  && i.ProductId == request.Id);
                
                if (thumbnailImage != null)
                {
                    thumbnailImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailImage.ImagePath = await SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new EShopException($"Cannot find a product: {productId}");
            
            _context.Products.Remove(product);

            var images = _context.ProductImages.Where(i =>i.ProductId == productId);
            foreach (var image in images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            

            if (product == null )
                throw new EShopException($"Cannot find a product with id: {productId}");

            product.Price = newPrice;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(int productId, int addedQuantity)
        {
            var product = await _context.Products.FindAsync(productId);


            if (product == null)
                throw new EShopException($"Cannot find a product with id: {productId}");

            product.Stock += addedQuantity;

            return await _context.SaveChangesAsync() > 0;
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

        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetManageProductPagingRequest request)
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

        public Task<int> AddImages(int productId, List<IFormFile> files)
        {
            throw new NotImplementedException();
        }

        public Task<int> RemoveImage(int imageId)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateImage(int imageId, string caption, bool isDefault)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProductImageViewModel>> GetListImages(int productId)
        {
            throw new NotImplementedException();
        }


        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
    }
}