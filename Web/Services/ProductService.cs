using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
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
    private SqliteConnection _db = db;

    /// <summary>
    /// Lists all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<Product> ListProducts()
    {
        var productsList = new List<Product>();

        var command = _db.CreateCommand();
        command.CommandText = @"SELECT products.product_id, products.brand, products.name, products.expiry, products.expiry_type, products.perishable from products;";

        using var reader = command.ExecuteReader();

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        var productId = reader.GetOrdinal("product_id");
        var brand = reader.GetOrdinal("brand");
        var name = reader.GetOrdinal("name");
        var expiry = reader.GetOrdinal("expiry");
        var expiryEnum = reader.GetOrdinal("expiry_type");
        var perishable = reader.GetOrdinal("perishable");

        while (reader.Read())
        {
            var product = new Product(
                reader.GetGuid(productId),
                reader.GetString(brand),
                reader.GetString(name),
                reader.GetValue(expiry) == DBNull.Value ? null : new DateTime(reader.GetInt64(perishable)),
                Enum.TryParse<ExpiryType>(reader.GetString(expiryEnum), out var expiryType) ? expiryType : null,
                reader.GetBoolean(perishable));

            productsList.Add(product);
        }
        reader.Close();

        return productsList;
    }

    /// <summary>
    /// Adds a new product to store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProduct(Product product)
    {
        var vc = new ValidationContext(product);
        Validator.ValidateObject(product, vc, true);

        var command = _db.CreateCommand();
        command.CommandText = "INSERT INTO products(product_id, brand, name, expiry, expiry_type, perishable) " +
                              $"VALUES (@id, @brand, @name, @expiry, @expiryType, @perishable);";
        command.Parameters.AddWithValue("@id", product.Id);
        command.Parameters.AddWithValue("@brand", product.Brand);
        command.Parameters.AddWithValue("@name", product.Name);
        command.Parameters.AddWithValue("@expiry", product.Expiry.HasValue ? product.Expiry : DBNull.Value);
        command.Parameters.AddWithValue("@expiryType", product.ExpiryType);
        command.Parameters.AddWithValue("@perishable", product.Perishable);

        var resp = command.ExecuteNonQuery();
        if (resp != 1)
        {
            Console.WriteLine($"Failed to insert product {product.Brand} {product.Name}");
        }
    }
}