namespace Warehouse.Manager.Models
{
    public class ProductModalVM
    {
    public Product? Product { get; set; } = new Product();
    public List<ProductType>? ProductTypes { get; set; }
  }
}
