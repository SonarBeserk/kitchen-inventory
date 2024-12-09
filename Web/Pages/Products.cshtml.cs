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
    private readonly IAccountService _accountService;

    public ProductsModel(ILogger<ProductsModel> logger, IProductService productService, ILocationService locationService, IAccountService accountService)
    {
        _logger = logger;
        _productService = productService;
        _locationService = locationService;
        _accountService = accountService;
        CurrentProduct = new Product();

        CanEdit = _accountService.IsAdmin();
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
    /// The product being edited in the product form
    /// </summary>
    [BindProperty]
    public Product CurrentProduct { get; set; }

    public bool IsEditingProduct { get; private set; }

    public bool CanEdit { get; private set; }

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

    public IActionResult OnGetProductForm(Guid productId)
    {
        IsEditingProduct = !IsEditingProduct;

        if (productId != Guid.Empty)
        {
            _logger.Log(LogLevel.Information, "Product id {id}", productId);
            try
            {
                CurrentProduct = _productService.GetProduct(productId) ?? new Product();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get product details {message}", e.Message);
            }
        }

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

        _logger.Log(LogLevel.Information, "New product being added {brand} {product}", CurrentProduct.Brand, CurrentProduct.Name);
        _productService.AddProductToInventory(CurrentProduct);

        ModelState.Clear(); // Allow inserting more products by cleaning up form and leaving it open for more entries
        CurrentProduct = new Product();

        _logger.Log(LogLevel.Information, "Continuing editing");
        // Keep form open if requested
        if (keepEditing)
        {
            IsEditingProduct = true;
        }

        return Partial("_ProductForm", this);
    }
}
