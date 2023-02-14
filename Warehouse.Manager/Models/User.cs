using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Warehouse.Manager.Models
{
  public class User
  {
    [Display(Name = "Email адрес (*)")]
    [Required(ErrorMessage = "Задължително поле!"), EmailAddress(ErrorMessage = "Не валиден email адрес!")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [Display(Name = "Потребителско Име (*)")]
    [Required(ErrorMessage = "Задължително поле!")]
    [RegularExpression(@"[A-Za-z_]{5,15}", ErrorMessage = "Името не спазва изискванията!")]
    public string Username { get; set; }

    [Display(Name = "Парола (*)")]
    [Required(ErrorMessage = "Задължително поле!"), PasswordPropertyText]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@\-_~|])[A-Za-z\d@\-_~|]{6,20}$", ErrorMessage = "Паролата не спазва изискванията!")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Потвърждаване на парола (*)")]
    [Required(ErrorMessage = "Задължително поле!"), PasswordPropertyText]
    [Compare("Password",ErrorMessage = "Паролите не съвпадат!")]
    [DataType(DataType.Password)]
    public string PasswordMatch { get; set; }

    [Phone, DefaultValue(null)]
    [RegularExpression(@"^(\+\d{1,3}\s)?\(?\d{3,4}\)?[\s.-]?\d{3}[\s.-]?\d{3,4}", ErrorMessage = "Невалиден номер!")]
    [DataType(DataType.PhoneNumber)]
    public string? Phone { get; set; } = string.Empty;

  }
}
