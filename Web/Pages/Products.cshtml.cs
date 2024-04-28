using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;
using Web.Services;

namespace Web.Pages;

public class ProductsModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;
    private readonly IProductService _productService;

    public ProductsModel(ILogger<PrivacyModel> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public void OnGet()
    {
        var products = _productService.ListProducts();
        foreach (var productModel in products)
        {
            _logger.Log(LogLevel.Information, "Product: {brand} {name}", productModel.Brand, productModel.Name);
        }
    }
}