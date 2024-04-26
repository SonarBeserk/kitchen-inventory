using System.ComponentModel.DataAnnotations;
using System.Data;
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
    public List<ProductModel> ListProducts();

    /// <summary>
    /// Adds a new product to store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProduct(ProductModel product);
}

public class ProductService(SqliteConnection db) : IProductService
{
    private SqliteConnection _db = db;

    /// <summary>
    /// Lists all tracked products
    /// </summary>
    /// <returns>All products being stored</returns>
    public List<ProductModel> ListProducts()
    {
        var productsList = new List<ProductModel>();

        var command = _db.CreateCommand();
        command.CommandText = @"SELECT products.product_id, products.brand, products.name, products.expiry, products.expiry_type, products.perishable, products.location_id from products;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var product = new ProductModel(
                (Guid)reader["product_id"],
                (string)reader["brand"],
                (string)reader["name"],
                (DateTime?)reader["expiry"],
                (ExpiryType?)reader["expiry_type"],
                (bool)reader["perishable"],
                (Guid)reader["location_id"]);

            productsList.Add(product);
        }

        return productsList;
    }

    /// <summary>
    /// Adds a new product to store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProduct(ProductModel product)
    {
    }
}