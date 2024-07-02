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

public class LocationsModel : PageModel
{
    private readonly ILogger<LocationsModel> _logger;
    private readonly ILocationService _locationService;

    public LocationsModel(ILogger<LocationsModel> logger, ILocationService locationService)
    {
        _logger = logger;
        _locationService = locationService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<Location>? LocationResults { get; private set; }

    public IActionResult OnGet()
    {
        var locations = _locationService.ListLocations();
        LocationResults = string.IsNullOrEmpty(Query) ?
            locations :
            locations.Where(l =>
                l.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) ||
                l.Description.Contains(Query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (!Request.IsHtmx())
        {
            return Page();
        }

        Response.Htmx(h =>
        {
            h.PushUrl(Request.GetEncodedUrl());
        });

        return Partial("_LocationTableResults", this);
    }
}
