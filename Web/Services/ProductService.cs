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
    /// Lists all products currently tracked by the inventory including locations
    /// </summary>
    /// <returns></returns>
    public List<Product> ListInventoriedProducts();

    /// <summary>
    /// Gets the product details for a specific product
    /// </summary>
    /// <param name="productId">The id of the product to get the details of</param>
    /// <returns>The product if found, otherwise null</returns>
    public Product? GetProductDetails(Guid productId);

    /// <summary>
    /// Adds a new product to the store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProductDetails(Product product);
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

    public List<Product> ListInventoriedProducts()
    {
        var products = new List<Product>();

        var command = db.CreateCommand();
        command.CommandText = @"
            SELECT inventory.product_id, inventory.expiry, inventory.expiry_type, inventory.perishable, inventory.amount, inventory.location_id
            from inventory
            INNER JOIN main.products p on p.product_id = inventory.product_id;";

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
        var expiry = reader.GetOrdinal("expiry");
        var expiryType = reader.GetOrdinal("expiry_type");
        var perishable = reader.GetOrdinal("perishable");
        var amount = reader.GetOrdinal("amount");
        var locationId = reader.GetOrdinal("location_id");

        while (reader.Read())
        {
            var product = new Product(
                reader.GetGuid(productId),
                reader.GetString(brand),
                reader.GetString(name),
                reader.GetGuid(locationId),
                reader.GetBoolean(perishable),
                reader.GetInt64(amount),
                reader.GetDateTime(expiry),
                (ExpiryType?)Enum.ToObject(typeof(ExpiryType), reader.GetInt64(expiryType))
                );
            products.Add(product);
        }
        reader.Close();

        return products;
    }

    /// <summary>
    /// Gets the product details for a specific product
    /// </summary>
    /// <param name="productId">The id of the product to get the details of</param>
    /// <returns>The product if found, otherwise null</returns>
    public Product? GetProductDetails(Guid productId)
    {
        var command = db.CreateCommand();
        command.CommandText = "SELECT products.product_id, products.brand, products.name from products WHERE products.product_id = @id;";
        command.Parameters.AddWithValue("@id", productId);

        using var reader = command.ExecuteReader();

        // Handle when no products are returned
        if (!reader.HasRows)
        {
            return null;
        }
        
        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        var productIdOrd = reader.GetOrdinal("product_id");
        var brandOrd = reader.GetOrdinal("brand");
        var nameOrd = reader.GetOrdinal("name");

        reader.Read();
        var product = new Product(
            reader.GetGuid(productIdOrd),
            reader.GetString(brandOrd),
            reader.GetString(nameOrd)
        );

        if (reader.Read())
        {
            reader.Close();
            throw new InvalidOperationException("Multiple products found");
        }
        
        reader.Close();

        return product;
    }

    /// <summary>
    /// Adds a new product to the store
    /// </summary>
    /// <param name="product">The new product</param>
    public void AddProductDetails(Product product)
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