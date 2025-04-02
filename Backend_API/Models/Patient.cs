using System.ComponentModel.DataAnnotations;

namespace Backend_API.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public User User { get; set; }
    }
}