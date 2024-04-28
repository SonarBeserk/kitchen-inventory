using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Web.Models;

namespace Web.Services;

public interface ILocationService
{
    public List<Location> ListLocations();
}

public class LocationService(SqliteConnection db) : ILocationService
{
    public List<Location> ListLocations()
    {
        var locations = new List<Location>();

        var command = db.CreateCommand();
        command.CommandText = @"SELECT locations.location_id, locations.name, locations.description from locations;";

        using var reader = command.ExecuteReader();

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        var locationId = reader.GetOrdinal("location_id");
        var name = reader.GetOrdinal("name");
        var description = reader.GetOrdinal("description");

        while (reader.Read())
        {
            var location = new Location(
                reader.GetGuid(locationId),
                reader.GetString(name),
                reader.GetString(description));
            
            locations.Add(location);
        }
        reader.Close();

        return locations;
    }

