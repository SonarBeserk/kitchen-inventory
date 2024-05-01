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
    private readonly ILocationService _locationService;

    public ProductsModel(ILogger<PrivacyModel> logger, IProductService productService, ILocationService locationService)
    {
        _logger = logger;
        _productService = productService;
        _locationService = locationService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Product>? ProductResults { get; private set; }
    public Dictionary<Guid, Location>? Locations {get; private set; }

    public IActionResult OnGet()
    {
        var products = _productService.ListProducts();
        Locations = _locationService.ListLocations().ToDictionary(l => l.Id, l => l);
        ProductResults = string.IsNullOrEmpty(Query)
            ? products
            : products.Where(p =>
                p.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Contains(Query, StringComparison.OrdinalIgnoreCase))
                .ToList();

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