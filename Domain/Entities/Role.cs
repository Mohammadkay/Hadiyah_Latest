using System.ComponentModel.DataAnnotations;

namespace HadiyahDomain.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } // "Admin", "Customer"

        public virtual ICollection<User> Users { get; set; }
    }
}
