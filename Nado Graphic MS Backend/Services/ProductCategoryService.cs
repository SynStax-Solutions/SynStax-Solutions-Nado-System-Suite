using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Services
{
    // Business logic for product categories - kept separate from the
    // controller so validation rules live in one place.
    public class ProductCategoryService
    {
        private readonly BMSDbContext _context;

        public ProductCategoryService(BMSDbContext context)
        {
            _context = context;
        }
        //----------------------------------------------------------------------------------
        // Returns every category, active or not 
        public async Task<List<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories.ToListAsync();
        }
        //----------------------------------------------------------------------------------
        public async Task<ProductCategory?> GetCategoryAsync(int id)
        {
            return await _context.ProductCategories.FindAsync(id);
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
            var nameTaken = await _context.ProductCategories
                .AnyAsync(c => c.CategoryName == category.CategoryName);

            if (nameTaken)
            {
                return false;
            }

            _context.ProductCategories.Add(category);

            await _context.SaveChangesAsync();

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

            var existingCategory = await _context.ProductCategories.FindAsync(id);

            if (existingCategory == null)
            {
                return false;
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;
            existingCategory.IsActive = category.IsActive;

            await _context.SaveChangesAsync();

            return true;
        }
        //----------------------------------------------------------------------------------
        // Hard-deletes a category.
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.ProductCategories.FindAsync(id);

            if (category == null)
            {
                return false;
            }

            _context.ProductCategories.Remove(category);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------