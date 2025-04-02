using Backend_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend_API.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string UserType { get; set; }
        public Doctor? Doctor { get; set; } // Đảm bảo là nullable
        public Patient? Patient { get; set; } // Đảm bảo là nullable
    }
}