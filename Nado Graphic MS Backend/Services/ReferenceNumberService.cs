using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Data;
using System.Data;

namespace Nado_Graphics_MS_Backend.Services
{
    //----------------------------------------------------------------------------------------------------------
    //Shared service so Quote and Invoice pull from the same reference counter and never collide
    public class ReferenceNumberService
    {
        private readonly BMSDbContext _context;

        public ReferenceNumberService(BMSDbContext context)
        {
            _context = context;
        }
        //------------------------------------------------------------------------------------------------------
        //Pulls the next value from the shared SQL sequence
        private async Task<int> GetNextCounterAsync()
        {
            var connection = _context.Database.GetDbConnection();

            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT NEXT VALUE FOR dbo.ReferenceNumberSequence";

                var result = await command.ExecuteScalarAsync();

                return Convert.ToInt32(result);
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        //-----------------------------------------------------------------------
        //Builds a reference number like "JOHN0007", first 4 letters of the client's
        //first name, uppercased and the shared counter
        public async Task<string> GenerateReferenceNumberAsync(string clientFirstName)
        {
            var counter = await GetNextCounterAsync();
            var prefix = BuildNamePrefix(clientFirstName);

            return $"{prefix}{counter:D4}";
        }

        private static string BuildNamePrefix(string firstName)
        {
            var letters = new string((firstName ?? string.Empty)
                .Where(char.IsLetter)
                .ToArray())
                .ToUpperInvariant();

            if (letters.Length == 0)
            {
                letters = "CLNT";
            }

            if (letters.Length < 4)
            {
                letters = letters.PadRight(4, 'X');
            }

            return letters.Substring(0, 4);
        }
    }
}
//--------------------------------------------------END OF FILE------------------------------------------------------------