namespace Nado_Graphics_MS_Backend.Security
{
    //This class stores the names of the values we save inside the login token (JWT).
    public static class AppClaims
    {
        //The user's ID.
        public const string UserId = "sub";

        //The user's full name.
        public const string Name = "name";

        //The user's email address.
        public const string Email = "email";

        //The user's role.
        public const string Role = "role";
    }
}