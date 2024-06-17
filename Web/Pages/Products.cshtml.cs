using Htmx;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;
using Web.Services;

namespace Web.Pages;

public class ProductsModel : PageModel
{
    private readonly ILogger<ProductsModel> _logger;
    private readonly IProductService _productService;
    private readonly ILocationService _locationService;

    public ProductsModel(ILogger<ProductsModel> logger, IProductService productService, ILocationService locationService)
    {
        _logger = logger;
        _productService = productService;
        _locationService = locationService;
        NewProduct = new Product();
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Product>? ProductResults { get; private set; }
    public Dictionary<Guid, Location>? Locations {get; private set; }

    [BindProperty]
    public Product NewProduct { get; set; }

    public bool IsEditingProduct { get; private set; }

    public IActionResult OnGet()
    {
        var products = _productService.ListAllDetails();
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

    public IActionResult OnGetProductForm()
    {
        IsEditingProduct = !IsEditingProduct;

        if (!Request.IsHtmx())
        {
            return Page();
        }

        Response.Htmx(h =>
        {
            h.PushUrl(Request.GetEncodedUrl());
        });

        return Partial("_ProductForm", this);
    }

    public IActionResult OnPostProduct()
    {
        if (!Request.IsHtmx())
        {
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Partial("_ProductForm", this);
        }

        _logger.Log(LogLevel.Information, "New product being added {0} {1}", NewProduct.Brand, NewProduct.Name);
        _productService.AddProductToInventory(NewProduct);

        // Allow inserting more products by cleaning up form and leaving it open for more entries
        ModelState.Clear();
        IsEditingProduct = true;
        NewProduct = new Product();

        return Partial("_ProductForm", this);
    }
}