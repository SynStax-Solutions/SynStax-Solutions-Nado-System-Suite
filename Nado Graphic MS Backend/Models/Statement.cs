using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nado_Graphics_MS_Backend.Models
{
    // EF Core entity mapped to dbo.Statement. Exactly one of ClientID / OrganisationID is set.
    public class Statement
    {
        [Key]
        public int StatementID { get; set; }

        public int? ClientID { get; set; }

        public int? OrganisationID { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatementReference { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime DateGenerated { get; set; } = DateTime.Now;
    }
}