using System.ComponentModel.DataAnnotations;

namespace Backend_API.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        public int UserID { get; set; }

        [Required]
        public int ResumeNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        [Required]
        public int SalaryCoefficient { get; set; }

        [Required]
        [StringLength(50)]
        public string Shift { get; set; }

        public User User { get; set; }
    }
}