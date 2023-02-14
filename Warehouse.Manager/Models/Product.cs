using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Warehouse.Manager.Models
{
  public class Product
  {
    public int pID { get; set; }
    [Display(Name = "Категория")]
    public string pType { get; set; }
    [Display(Name = "Име")]
    public string pName { get; set; }
    [Display(Name = "Описание")]
    public string? pDesc { get; set; }
    [Display(Name = "Снимка")]
    public byte[] pImg { get; set; } = new byte[0];
    [Display(Name = "Купено на [BGN]")]
    public double pBPrice { get; set; }
    [Display(Name = "Продава на [BGN]")]
    public double pSPrice { get; set; }
    [Display(Name = "Количество")]
    public int pAmount { get; set; }

    private string imgB64 = string.Empty;
    public string ImageBase64
    {
      get => imgB64 = Convert.ToBase64String(pImg);
      set => pImg = Convert.FromBase64String(value);
    }
  }
}
