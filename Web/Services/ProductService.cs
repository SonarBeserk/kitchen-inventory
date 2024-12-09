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

/// <summary>
/// Represents the interface for the service that manages products
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Lists details of all tracked products
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <returns>All products being stored</returns>
    public List<Product> ListAllDetails();

    /// <summary>
    /// Lists all products currently tracked by the inventory including locations
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <returns>The list of track products with all details fetched</returns>
    public List<Product> ListInventoriedProducts();

    /// <summary>
    /// Gets the product details for a specific product
    /// </summary>
    /// <param name="productId">The id of the product to get the details of</param>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <exception cref="InvalidOperationException">Multiple product results are found which should not happen</exception>
    /// <returns>The product if found, otherwise null</returns>
    public Product? GetProduct(Guid productId);

    /// <summary>
    /// Adds a new product to the database
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <param name="product">The new product</param>
    public void AddProduct(Product product);

    /// <summary>
    /// Adds a new product to the store including details such as location, expiry, and amount
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <param name="product">The new product to add to the inventory</param>
    public void AddProductToInventory(Product product);
}

public class ProductService(SqliteConnection db) : IProductService
{
    /// <summary>
    /// Lists details of all tracked products
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <returns>All products being stored</returns>
    public List<Product> ListAllDetails()
    {
        var products = new List<Product>();

        var command = db.CreateCommand();
        command.CommandText = @"SELECT products.product_id, products.brand, products.name FROM products;";

        using var reader = command.ExecuteReader();

        // Handle when no products are returned
        if (!reader.HasRows)
        {
            return products;
        }

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        int productId = reader.GetOrdinal("product_id");
        int brand = reader.GetOrdinal("brand");
        int name = reader.GetOrdinal("name");

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
    /// Lists all products currently tracked by the inventory including locations
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <returns>The list of track products with all details fetched</returns>
    public List<Product> ListInventoriedProducts()
    {
        var products = new List<Product>();

        var command = db.CreateCommand();
        command.CommandText = @"
            SELECT inventory.product_id, brand, name, inventory.expiry, inventory.expiry_type, inventory.perishable, inventory.amount, inventory.location_id
            FROM inventory
            INNER JOIN main.products p on p.product_id = inventory.product_id;";

        using var reader = command.ExecuteReader();

        // Handle when no products are returned
        if (!reader.HasRows)
        {
            return products;
        }

        // SqlDataReader is based on ordinal values, using the string index does this anyway,
        // but we can use the helper functions if we grab the ordinal values directly.
        int productId = reader.GetOrdinal("product_id");
        int brand = reader.GetOrdinal("brand");
        int name = reader.GetOrdinal("name");
        int expiry = reader.GetOrdinal("expiry");
        int expiryType = reader.GetOrdinal("expiry_type");
        int perishable = reader.GetOrdinal("perishable");
        int amount = reader.GetOrdinal("amount");
        int locationId = reader.GetOrdinal("location_id");

        while (reader.Read())
        {
            var product = new Product(
                reader.GetGuid(productId),
                reader.GetString(brand),
                reader.GetString(name),
                reader.IsDBNull(locationId) ? null : reader.GetGuid(locationId),
                reader.GetBoolean(perishable),
                reader.GetInt64(amount),
                reader.IsDBNull(expiry) ? null : reader.GetDateTime(expiry),
                reader.IsDBNull(expiryType) ? null : (ExpiryType?)Enum.ToObject(typeof(ExpiryType), reader.GetInt64(expiryType))
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
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <exception cref="InvalidOperationException">Multiple product results are found which should not happen</exception>
    /// <returns>The product if found, otherwise null</returns>
    public Product? GetProduct(Guid productId)
    {
        var command = db.CreateCommand();
        command.CommandText = "SELECT products.product_id, products.brand, products.name FROM products WHERE products.product_id = @id LIMIT 1;";
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
    /// Adds a new product to the database
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <param name="product">The new product</param>
    public void AddProduct(Product product)
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

    /// <summary>
    /// Adds a new product to the store including details such as location, expiry, and amount
    /// </summary>
    /// <exception cref="SqliteException">Database exception occurred</exception>
    /// <param name="product">The new product to add to the inventory</param>
    public void AddProductToInventory(Product product)
    {
        var vc = new ValidationContext(product);
        Validator.ValidateObject(product, vc, true);

        // TODO: Check for an existing product by brand / name to prevent some duplication
        var command = db.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM products WHERE products.product_id = @id)";
        command.Parameters.AddWithValue("@id", product.Id);

        var selectResp = command.ExecuteScalar();
        if (selectResp is not long i)
        {
            throw new InvalidOperationException("Checking for existing product did not return an int");
        }

        command.Dispose(); // Reuse the command variable but release the old resources

        // Product details need to be saved
        if (i == 0)
        {
            AddProduct(product);
        }

        command = db.CreateCommand();
        command.CommandText =
            "INSERT INTO inventory(inventory_id, product_id, expiry, expiry_type, perishable, amount, location_id) " +
            "VALUES (@inventoryId, @productId, @expiry, @expiryType, @perishable, @amount, @locationId)";
        command.Parameters.AddWithValue("@inventoryId", Guid.NewGuid()); // Inventory ids are just primary keys, so we can generate them
        command.Parameters.AddWithValue("@productId", product.Id);
        command.Parameters.AddWithValue("@expiry", product.Expiry.HasValue ? product.Expiry.Value : DBNull.Value);
        command.Parameters.AddWithValue("@expiryType", product.ExpiryType.HasValue ? product.ExpiryType.Value : DBNull.Value);
        command.Parameters.AddWithValue("@perishable", product.Perishable);
        command.Parameters.AddWithValue("@amount", product.Amount);
        command.Parameters.AddWithValue("@locationId", product.LocationId.HasValue ? product.LocationId.Value : DBNull.Value);

        int resp = command.ExecuteNonQuery();
        if (resp != 1)
        {
            Console.WriteLine("Failed to add product to inventory {0} {1}", product.Brand, product.Name);
        }
    }
}
