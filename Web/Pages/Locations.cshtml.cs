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
