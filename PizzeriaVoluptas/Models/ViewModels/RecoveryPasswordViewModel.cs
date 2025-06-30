using System.ComponentModel.DataAnnotations;

namespace PizzeriaVoluptas.Models.ViewModels
{
    public class RecoveryPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
