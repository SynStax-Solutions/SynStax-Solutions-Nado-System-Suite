using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;

namespace Nado_Graphics_MS_Backend.Services
{
    // Business logic for product categories - kept separate from the
    // controller so validation rules live in one place.
    public class ProductCategoryService
    {
        private readonly IProductCategoryRepository _repo;

        public ProductCategoryService(IProductCategoryRepository repo)
        {
            _repo = repo;
        }
        //----------------------------------------------------------------------------------
        // Returns every category, active or not 
        public async Task<List<ProductCategory>> GetCategoriesAsync()
        {
            return await _repo.GetAllAsync();
        }
        //----------------------------------------------------------------------------------
        public async Task<ProductCategory?> GetCategoryAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
        //----------------------------------------------------------------------------------
        // Creates a new category after checking the name isn't already used.
        public async Task<bool> CreateCategoryAsync(ProductCategory category)
        {
            // Force 0 so EF treats this as a new insert rather than trying
            // to update whatever ID the caller might have sent.
            category.ProductCategoryID = 0;

            //CategoryName must be unique per the schema - check before inserting so
            //we return a clean false instead of an unhandled SQL exception.
            var nameTaken = await _repo.NameExistsAsync(category.CategoryName);

            if (nameTaken)
            {
                return false;
            }

            await _repo.AddAsync(category);

            await _repo.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Updates an existing category's editable fields. 
        public async Task<bool> UpdateCategoryAsync(int id, ProductCategory category)
        {
            if (id != category.ProductCategoryID)
            {
                return false;
            }

            var existingCategory = await _repo.GetByIdAsync(id);

            if (existingCategory == null)
            {
                return false;
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;
            existingCategory.IsActive = category.IsActive;

            await _repo.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Hard-deletes a category.
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);

            if (category == null)
            {
                return false;
            }

            _repo.Remove(category);

            await _repo.SaveChangesAsync();

            return true;
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------