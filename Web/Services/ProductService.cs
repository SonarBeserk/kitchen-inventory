using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Web.Models;

namespace Web.Services;

/// <summary>
/// Represents the interface for the service that manages products
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lists all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<Product> ListProducts();

    /// <summary>
    /// Adds a new product to store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProduct(Product product);
}

public class ProductService(SqliteConnection db) : IProductService
{
    /// <summary>
    /// Lists all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<Product> ListProducts()
    {
        var products = new List<Product>();

        var command = db.CreateCommand();
        command.CommandText = @"SELECT
products.product_id,
products.brand,
products.name,
products.expiry,
products.expiry_type,
products.perishable,
products.amount,
products.location_id
from products;";

        using var reader = command.ExecuteReader();

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        var productId = reader.GetOrdinal("product_id");
        var brand = reader.GetOrdinal("brand");
        var name = reader.GetOrdinal("name");
        var expiry = reader.GetOrdinal("expiry");
        var expiryEnum = reader.GetOrdinal("expiry_type");
        var perishable = reader.GetOrdinal("perishable");
        var amount = reader.GetOrdinal("amount");
        var locationId = reader.GetOrdinal("location_id");

        while (reader.Read())
        {
            var product = new Product(
                reader.GetGuid(productId),
                reader.GetString(brand),
                reader.GetString(name),
                reader.GetValue(expiry) == DBNull.Value ? null : new DateTime(reader.GetInt64(perishable)),
                reader.GetValue(expiryEnum) == DBNull.Value ? null : Enum.TryParse<ExpiryType>(reader.GetString(expiryEnum), out var expiryType) ? expiryType : null,
                reader.GetBoolean(perishable),
                reader.GetInt64(amount),
                reader.GetValue(locationId) == DBNull.Value ? null : reader.GetGuid(locationId));

            products.Add(product);
        }
        reader.Close();

        return products;
    }

    /// <summary>
    /// Adds a new product to store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProduct(Product product)
    {
        var vc = new ValidationContext(product);
        Validator.ValidateObject(product, vc, true);

        var command = db.CreateCommand();
        command.CommandText = "INSERT INTO products(product_id, brand, name, expiry, expiry_type, perishable, amount, location_id) " +
                              "VALUES (@id, @brand, @name, @expiry, @expiryType, @perishable, @amount, @locationid);";
        command.Parameters.AddWithValue("@id", product.Id);
        command.Parameters.AddWithValue("@brand", product.Brand);
        command.Parameters.AddWithValue("@name", product.Name);
        command.Parameters.AddWithValue("@expiry", product.Expiry.HasValue ? product.Expiry : DBNull.Value);
        command.Parameters.AddWithValue("@expiryType", product.ExpiryType.HasValue ? product.ExpiryType : DBNull.Value);
        command.Parameters.AddWithValue("@perishable", product.Perishable);
        command.Parameters.AddWithValue("@amount", product.Amount);
        command.Parameters.AddWithValue("@locationid", product.LocationId.HasValue ? product.LocationId : DBNull.Value);

        var resp = command.ExecuteNonQuery();
        if (resp != 1)
        {
            Console.WriteLine("Failed to insert product {0} {1}", product.Brand, product.Name);
        }
    }
}