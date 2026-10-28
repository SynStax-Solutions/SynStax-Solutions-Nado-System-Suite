using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles product business logic.
    public class ProductService
    {
        private readonly IRepository<Product> _products;
        private readonly IProductCategoryRepository _categories;

        public ProductService(IRepository<Product> products, IProductCategoryRepository categories)
        {
            _products = products;
            _categories = categories;
        }

        //----------------------------------------------------------------------------------
        // Returns every product, active or inactive.
        public async Task<List<Product>> GetProductsAsync()
        {
            return await _products.GetAllAsync();
        }
        //----------------------------------------------------------------------------------
        public async Task<Product?> GetProductAsync(int id)
        {
            return await _products.GetByIdAsync(id);
        }
        //----------------------------------------------------------------------------------
        // Creates a new product after validating if its category exists and its
        // price isn't negative.
        public async Task<bool> CreateProductAsync(Product product)
        {
            product.ProductID = 0;
            product.CreatedAt = DateTime.Now;

            //ProductCategoryID is required - confirm the category actually exists.
            var category = await _categories.GetByIdAsync(product.ProductCategoryID);

            if (category == null)
            {
                return false;
            }


            if (product.Price < 0)
            {
                return false;
            }

            await _products.AddAsync(product);

            await _products.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Updates an existing product's editable fields.
        public async Task<bool> UpdateProductAsync(int id, Product product)
        {
            if (id != product.ProductID)
            {
                return false;
            }

            var existingProduct = await _products.GetByIdAsync(id);

            if (existingProduct == null)
            {
                return false;
            }

            var category = await _categories.GetByIdAsync(product.ProductCategoryID);

            if (category == null)
            {
                return false;
            }



            existingProduct.ProductCategoryID = product.ProductCategoryID;
            existingProduct.ProductName = product.ProductName;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.IsActive = product.IsActive;

            await _products.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Hard-deletes a product.
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _products.Remove(product);
            await _products.SaveChangesAsync();

            return true;
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------