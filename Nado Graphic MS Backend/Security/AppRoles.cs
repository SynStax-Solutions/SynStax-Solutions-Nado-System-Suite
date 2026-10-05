namespace Nado_Graphics_MS_Backend.Security
{
    //This class stores the role names and the rules for who can manage who.
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

        //This checks if the role is one of the valid roles.
        public static bool IsValid(string? role) => role != null && All.Contains(role);

        //This checks if a user can manage another user.
        //Manager can manage everyone, the rest can only manage roles lower than theirs.
        public static bool CanManage(string? actorRole, string? targetRole) =>
            actorRole == Manager || (Rank(actorRole) > Rank(targetRole) && Rank(targetRole) > 0);

        //This checks if a user is allowed to give someone a certain role.
        public static bool CanAssign(string? actorRole, string? newRole) =>
            IsValid(newRole) && CanManage(actorRole, newRole);
    }
}