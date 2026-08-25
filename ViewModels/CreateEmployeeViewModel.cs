using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.ViewModels
{
    public class CreateEmployeeViewModel
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, MinLength(6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = "Employee";
    }
}
