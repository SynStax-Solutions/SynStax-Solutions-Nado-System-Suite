namespace Nado_Graphics_MS_Backend.Models
{
    //This class is returned by the services to show if something worked, with a message.
    public class ServiceResult
    {
        //This is true if the action worked.
        public bool Success { get; init; }

        //This is the message shown to the user.
        public string? Message { get; init; }

        //This creates a successful result.
        public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };

        //This creates an unsuccessful result.
        public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
    }
}