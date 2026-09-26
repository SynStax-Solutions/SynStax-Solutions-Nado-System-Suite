namespace Nado_Graphics_MS_Frontend.Security
{
    //This class stores the role names and rules. It matches the one in the backend.
    //The Owner uses the Manager role.
    public static class AppRoles
    {
        //The three roles in the system.
        public const string Manager = "Manager";
        public const string Admin = "PA/Admin";
        public const string Designer = "Graphic Designer";

        //This is used in [Authorize] so both Manager and PA/Admin are allowed.
        public const string ManagerOrAdmin = Manager + "," + Admin;

        //This is a list of all the valid roles.
        public static readonly string[] All = { Manager, Admin, Designer };

        //This gives each role a number so we can compare them. Higher number = higher role.
        public static int Rank(string? role) => role switch
        {
            Manager => 3,
            Admin => 2,
            Designer => 1,
            _ => 0
        };

        //This checks if a user can manage another user.
        //Manager can manage everyone, the rest can only manage roles lower than theirs.
        public static bool CanManage(string? actorRole, string? targetRole) =>
            actorRole == Manager || (Rank(actorRole) > Rank(targetRole) && Rank(targetRole) > 0);

        //This gets the roles the user is allowed to give out (used in the role dropdowns).
        public static IEnumerable<string> AssignableBy(string? actorRole) =>
            All.Where(r => CanManage(actorRole, r));

        //This decides which dashboard each role goes to after logging in.
        public static (string Action, string Controller) DashboardFor(string? role) => role switch
        {
            Manager => ("Dashboard", "Owner"),
            Admin => ("Dashboard", "Admins"),
            _ => ("Dashboard", "Designers")
        };
    }
}
// ------------------------- END OF FILE ------------------------- //