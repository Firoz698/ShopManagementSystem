using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ShopManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi phone number (e.g. 017XXXXXXXX).")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current Password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required."), MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP Code is required."), StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP Code is required.")]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required."), MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? ProfilePhotoFile { get; set; }
        public string? CurrentPhotoUrl { get; set; }
    }
}