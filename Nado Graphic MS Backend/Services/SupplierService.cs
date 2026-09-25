using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class is the service that handles supplier business logic.
    public class SupplierService
    {
        //The below line stores the database context used to access the database.
        private readonly BMSDbContext _context;

        //The constructor receives the database context using dependency injection.
        public SupplierService(BMSDbContext context)
        {
            //Storing the injected database context.
            _context = context;
        }
        //----------------------------------------------------------------------------------//

        //This gets all the suppliers from the database.
        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            //Getting all supplier records and returning them as a list.
            return await _context.Suppliers.ToListAsync();
        }
        //----------------------------------------------------------------------------------//

        //This gets one supplier using the supplier's ID.
        public async Task<Supplier?> GetSupplierAsync(int id)
        {
            //Searching the database for the supplier with the specified ID.
            return await _context.Suppliers.FindAsync(id);
        }
        //----------------------------------------------------------------------------------//

        //This creates a new supplier and returns the operation result.
        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            //This resets the supplier ID so the database generates a new ID.
            supplier.SupplierID = 0;

            //This sets the creation date and time to the current date and time.
            supplier.CreatedAt = DateTime.Now;

            //This checks whether a supplier with the same name already exists.
            var nameExists = await _context.Suppliers.AnyAsync(s => s.SupplierName == supplier.SupplierName);

            //This checks whether the supplier name already exists.
            if (nameExists)
            {
                //This stops the process when a supplier with similar information exists.
                return false;
            }

            //This checks whether an email address is provided.
            if (!string.IsNullOrWhiteSpace(supplier.SupplierEmail))
            {
                //This checks whether a supplier with the same email already exists.
                var emailExists = await _context.Suppliers.AnyAsync(s => s.SupplierEmail == supplier.SupplierEmail);

                //This checks whether the supplier email already exists.
                if (emailExists)
                {
                    //This stops the process when a supplier with similar information exists.
                    return false;
                }
            }

            //Adds the new supplier to the db context.
            _context.Suppliers.Add(supplier);

            //Saves the new supplier to the database.
            await _context.SaveChangesAsync();

            //Returns a successful result.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //This updates an existing supplier and returns the operation result.
        public async Task<bool> UpdateSupplierAsync(int id, Supplier supplier)
        {
            //This checks if the URL ID matches the supplier's ID.
            if (id != supplier.SupplierID)
            {
                //This returns an unsuccessful result when the IDs do not match.
                return false;
            }

            //This searches the database for the existing supplier.
            var existingSupplier =
                await _context.Suppliers.FindAsync(id);

            //This checks whether the supplier does not exist.
            if (existingSupplier == null)
            {
                //Returns an unsuccessful result when the supplier is not found.
                return false;
            }

            //This checks whether another supplier already uses the same name.
            var nameExists = await _context.Suppliers.AnyAsync(s => s.SupplierName == supplier.SupplierName && s.SupplierID != id);

            //This checks whether the supplier name already exists.
            if (nameExists)
            {
                //This returns an unsuccessful result when similar supplier information exists.
                return false;
            }

            //This checks whether an email address is provided.
            if (!string.IsNullOrWhiteSpace(supplier.SupplierEmail))
            {
                //This checks whether another supplier already uses the same email.
                var emailExists = await _context.Suppliers.AnyAsync(s => s.SupplierEmail == supplier.SupplierEmail && s.SupplierID != id);

                //This checks whether the supplier email already exists.
                if (emailExists)
                {
                    //This returns an unsuccessful result when similar supplier information exists.
                    return false;
                }
            }

            //This updates the supplier name.
            existingSupplier.SupplierName = supplier.SupplierName;

            //This updates the supplier contact number.
            existingSupplier.ContactNumber = supplier.ContactNumber;

            //This updates the supplier email address.
            existingSupplier.SupplierEmail = supplier.SupplierEmail;

            //This updates the supplier address.
            existingSupplier.SupplierAddress = supplier.SupplierAddress;

            //This updates the supplier lead time.
            existingSupplier.LeadTime = supplier.LeadTime;

            //This updates the supplier service type.
            existingSupplier.ServiceType = supplier.ServiceType;

            //This saves the updated supplier to the database.
            await _context.SaveChangesAsync();

            //This returns a successful result.
            return true;
        }
        //----------------------------------------------------------------------------------//

        //This deletes an existing supplier and returns the operation result.
        public async Task<bool> DeleteSupplierAsync(int id)
        {
            //Searching the database for the supplier using the specified ID.
            var supplier = await _context.Suppliers.FindAsync(id);

            //Checking if the supplier does not exist.
            if (supplier == null)
            {
                //Returns an unsuccessful result when the supplier is not found.
                return false;
            }

            //Marks the supplier for deletion in the db context.
            _context.Suppliers.Remove(supplier);

            //Saves the changes to the database.
            await _context.SaveChangesAsync();

            //Returns a successful result.
            return true;
        }
    }
}
//--------------------------------------------------<>\\<>End of Program<>//<>------------------------------------------------------------//