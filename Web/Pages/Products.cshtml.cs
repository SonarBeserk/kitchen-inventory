using Htmx;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
        ProductResults = new List<Product>();
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Product> ProductResults { get; private set; }

    public IActionResult OnGet()
    {
        var products = _productService.ListProducts();
        ProductResults = string.IsNullOrEmpty(Query)
            ? products
            : products.Where(p =>
                p.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Contains(Query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _logger.Log(LogLevel.Information, "Results: {0}", Results.Count);

        if (!Request.IsHtmx())
        {
            return Page();
        }

        Response.Htmx(h =>
        {
            h.PushUrl(Request.GetEncodedUrl());
        });

        return Partial("_ProductTableResults", this);
    }
}