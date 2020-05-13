using System.ComponentModel.DataAnnotations;

namespace TwitterMVC.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(30)]
        public string NewPassword { get; set; }
    }
}