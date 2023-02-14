using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Manager.Models
{
  public class Login
  {
    [Required(ErrorMessage = "Задължително поле")]
    [Display(Name = "Потребителско Име или Email")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Задължително поле")]
    [Display(Name = "Парола")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Запомни ме")]
    public bool RememberLogin { get; set; }

    public string ReturnUrl { get; set; }
  }
}
