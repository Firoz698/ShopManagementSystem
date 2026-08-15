using System.ComponentModel.DataAnnotations;

namespace ShopManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "পুরো নাম দিন")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "সঠিক ইমেইল দিন")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "ফোন নম্বর দিন")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "সঠিক ফোন নম্বর দিন")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        // ✅ notun optional fields
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required, MinLength(6, ErrorMessage = "কমপক্ষে ৬ অক্ষর হতে হবে")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password", ErrorMessage = "পাসওয়ার্ড মিলছে না")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "পাসওয়ার্ড মিলছে না")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ✅ Email -> PhoneNumber
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "ফোন নম্বর দিন")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "সঠিক ফোন নম্বর দিন")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // ✅ Email -> PhoneNumber
    public class VerifyOtpViewModel
    {
        [Required] public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6, ErrorMessage = "৬ ডিজিটের OTP দিন")]
        public string OtpCode { get; set; } = string.Empty;
    }

    // ✅ Email -> PhoneNumber
    public class ResetPasswordViewModel
    {
        [Required] public string PhoneNumber { get; set; } = string.Empty;
        [Required] public string OtpCode { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "পাসওয়ার্ড মিলছে না")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class EditProfileViewModel
    {
        [Required] public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "সঠিক ফোন নম্বর দিন")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public IFormFile? ProfilePhotoFile { get; set; }
        public string? CurrentPhotoUrl { get; set; }
    }
}