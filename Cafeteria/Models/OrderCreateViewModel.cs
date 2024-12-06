using Cafeteria.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

public class OrderCreateViewModel
{ 
    public List<Product> Products {get; set;} = new List<Product>(); // Resgata todos os produtos do BD
    public IEnumerable<SelectListItem>? ProductsSelectList { get; set; } = new List<SelectListItem>(); // Lista de produtos selecionaveis em um Dropdown List
    public int SelectedProductId {get; set;} // Armazena o ID do produto selecionado no dropdown  
    public int Quantity {get; set;} // Quantidade do produto
    public decimal TotalPrice {get; set;} // Armazena o somatório dos preços dos produtos selecionados
    public string? Message {get; set;} // Utilizado para alertar o usuário de que a quantidade solicitada de um produto é insuficientet no estoque
}