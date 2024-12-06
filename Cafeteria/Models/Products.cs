using System.ComponentModel.DataAnnotations;

namespace Cafeteria.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
}