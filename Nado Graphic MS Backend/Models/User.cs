using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nado_Graphics_MS_Backend.Models
{
    //This class represents the User table in the database.
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        //This stores the hashed password, not the real password.
        //[JsonIgnore] stops the hash from being sent back by the API.
        [Required]
        [MaxLength(255)]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string UserRole { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //This becomes true once the user clicks the link in their verification email.
        public bool EmailVerified { get; set; } = false;

        //This stores the hashed version of the verification token.
        [MaxLength(64)]
        [JsonIgnore]
        public string? VerificationTokenHash { get; set; }

        //This stores when the verification link expires.
        [JsonIgnore]
        public DateTime? VerificationTokenExpiry { get; set; }

        //This counts how many wrong passwords were entered in a row.
        [JsonIgnore]
        public int FailedLoginAttempts { get; set; } = 0;

        //This stores when the account unlocks (null = not locked).
        [JsonIgnore]
        public DateTime? LockoutEnd { get; set; }

        //This stores when the password was last changed (UTC).
        //Any login made before this time is no longer accepted.
        [JsonIgnore]
        public DateTime? PasswordChangedAt { get; set; }

        //This stores the hashed version of the forgot password token.
        [MaxLength(64)]
        [JsonIgnore]
        public string? ResetTokenHash { get; set; }

        //This stores when the forgot password link expires.
        [JsonIgnore]
        public DateTime? ResetTokenExpiry { get; set; }

    }
}