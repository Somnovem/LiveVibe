using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LiveVibe.Server.Models.Tables
{
    [Table("Countries")]
    public class Country
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}