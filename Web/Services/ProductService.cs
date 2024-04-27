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
        while (reader.Read())
        {
            var product = new Product(
                Guid.Parse((string)reader["product_id"]),
                (string)reader["brand"],
                (string)reader["name"],
                reader["expiry"] == DBNull.Value ? null : new DateTime((long)reader["expiry"]),
                Enum.TryParse<ExpiryType>(reader["expiry_type"].ToString(), out var expiryType) ? expiryType : null,
                Convert.ToBoolean((long)reader["perishable"]));

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
        if (resp != 0)
        {
            Console.WriteLine($"Failed to insert product {product.Brand} {product.Name}");
        }
    }
}