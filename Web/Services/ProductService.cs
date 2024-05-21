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
    /// Lists details of all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<Product> ListAllDetails();

    /// <summary>
    /// Adds a new product to the store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddDetails(Product product);
}

public class ProductService(SqliteConnection db) : IProductService
{
    /// <summary>
    /// Lists all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<Product> ListAllDetails()
    {
        var products = new List<Product>();

        var command = db.CreateCommand();
        command.CommandText = @"SELECT products.product_id, products.brand, products.name from products;";

        using var reader = command.ExecuteReader();

        // Handle when no products are returned
        if (!reader.HasRows)
        {
            return products;
        }

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        var productId = reader.GetOrdinal("product_id");
        var brand = reader.GetOrdinal("brand");
        var name = reader.GetOrdinal("name");

        while (reader.Read())
        {
            var product = new Product(
                reader.GetGuid(productId),
                reader.GetString(brand),
                reader.GetString(name)
                );

            products.Add(product);
        }
        reader.Close();

        return products;
    }

    /// <summary>
    /// Adds a new product to the store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddDetails(Product product)
    {
        var vc = new ValidationContext(product);
        Validator.ValidateObject(product, vc, true);
        
        var command = db.CreateCommand();
        command.CommandText = "INSERT INTO products(product_id, brand, name) " +
                              "VALUES (@id, @brand, @name);";
        command.Parameters.AddWithValue("@id", product.Id);
        command.Parameters.AddWithValue("@brand", product.Brand);
        command.Parameters.AddWithValue("@name", product.Name);
        
        var resp = command.ExecuteNonQuery();
        if (resp != 1)
        {
            Console.WriteLine("Failed to insert product {0} {1}", product.Brand, product.Name);
        }
    }
}