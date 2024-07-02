// MIT License
// 
// Copyright (c) 2024 SonarBeserk
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

    /// <summary>
    /// Search string for product table (ex: Brand, Product Name)
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    /// <summary>
    /// The list of products from the database
    /// </summary>
    public List<Product>? ProductResults { get; private set; }

    /// <summary>
    /// The list of locations from the database
    /// </summary>
    public Dictionary<Guid, Location>? Locations { get; private set; }

    /// <summary>
    /// The new product created when the product form is posted
    /// </summary>
    [BindProperty]
    public Product NewProduct { get; set; }

    public bool IsEditingProduct { get; private set; }

    public IActionResult OnGet()
    {
        var products = _productService.ListInventoriedProducts();
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

        return Partial("_ProductForm", this);
    }

    public IActionResult OnPostProduct(bool keepEditing)
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

        ModelState.Clear(); // Allow inserting more products by cleaning up form and leaving it open for more entries
        NewProduct = new Product();

        _logger.Log(LogLevel.Information, "Continuing editing");
        // Keep form open if requested
        if (keepEditing)
        {
            IsEditingProduct = true;
        }

        return Partial("_ProductForm", this);
    }
}
