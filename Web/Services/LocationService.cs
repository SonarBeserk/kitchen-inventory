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

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Web.Models;

namespace Web.Services;

public interface ILocationService
{
    public List<Location> ListLocations();
    public void AddLocation(Location location);
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

    public void AddLocation(Location location)
    {
        var vc = new ValidationContext(location);
        Validator.ValidateObject(location, vc, true);

        var command = db.CreateCommand();
        command.CommandText = "INSERT INTO locations(location_id, name, description) " +
                              "VALUES (@id, @name, @description);";
        command.Parameters.AddWithValue("@id", location.Id);
        command.Parameters.AddWithValue("@name", location.Name);
        command.Parameters.AddWithValue("@description", location.Description);

        var resp = command.ExecuteNonQuery();
        if (resp != 1)
        {
            Console.WriteLine("Failed to insert location {0}", location.Name);
        }
    }
}
