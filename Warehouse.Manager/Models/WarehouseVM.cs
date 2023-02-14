namespace Warehouse.Manager.Models
{
  public class WarehouseVM
  {
    public SearchFilter SearchFilter { get; set; } = new ();
    public List<ProductType>? ProductTypes { get; set; }
    public List<Product>? Products { get; set; }
    public Product Product { get; set; } = new();
    public bool AddProductSelected { get; set; }
  }

  public class SearchFilter
  {
    public string? SearchText { get; set; }

    public int ProductTypeSelected { get; set; } = 0;
  }
}
