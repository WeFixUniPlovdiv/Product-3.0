using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Warehouse.Manager.Models;
using Warehouse.Manager.Services;

namespace Warehouse.Manager.Controllers
{
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public class WarehouseController : Controller
  {
    private IDbDataScopedService context;

    public WarehouseController(IDbDataScopedService db)
    {
      context = db;
    }
    public async Task<WarehouseVM> LoadWhVM(WarehouseVM? vm = null)
    {
      if (vm == null)
        vm = new();
      var searchText = vm.SearchFilter.SearchText;
      var productType = vm.SearchFilter.ProductTypeSelected;
      vm.Products = await context.GetProducts(productType.Equals(0) ? null : productType, searchText);
      vm.ProductTypes = await context.GetProductTypes();
      return vm;
    }
    public async Task<ProductModalVM> LoadProductVM(ProductModalVM? vm = null)
    {
      if (vm == null)
        vm = new();
      vm.ProductTypes = await context.GetProductTypes();
      return vm;
    }

    public async Task<IActionResult> Index([FromForm] WarehouseVM vm)
    {
      vm = await LoadWhVM(vm);
      return View(vm);
    }

    [HttpPost]
    public async Task<PartialViewResult> GetProductPartial(int id = 0)
    {
      var vm = await LoadProductVM();
      if (id != 0)
        vm.Product = await context.GetProduct(id);
      return PartialView("_ProductForm", vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromForm] ProductModalVM vm)
    {
      if (string.IsNullOrWhiteSpace(vm.Product.pDesc))
        vm.Product.pDesc = string.Empty;
      try
      {
        string res = await context.AddProduct(vm.Product);
      }
      catch
      {
        return View("Error");
      }
      return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProduct([FromForm] ProductModalVM vm)
    {
      if (string.IsNullOrWhiteSpace(vm.Product.pDesc))
        vm.Product.pDesc = string.Empty;
      try
      {
        var res = await context.UpdateProduct(vm.Product);
      }
      catch
      {
        return View("Error");
      }
      return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveProduct([FromForm] Product product)
    {
      try
      {
        await context.DeleteProduct(product.pID);
      }
      catch
      {
        return View("Error");
      }
      return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddProductCategory(string name)
    {
      var res = await context.AddProductType(name);
      return Ok(res);
    }
  }
}

