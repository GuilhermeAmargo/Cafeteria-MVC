using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cafeteria.Data;
using Cafeteria.Models;

namespace Cafeteria.Controllers
{
    public class OrderController : Controller
    {
        private readonly CafeteriaContext _context;

        public OrderController(CafeteriaContext context)
        {
            _context = context;
        }

        // GET: Order
        // Mostra a tela com todos os pedidos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Order.ToListAsync());
        }

        // GET: Order/Create
        // Tela de criação de novo pedido
        public async Task<IActionResult> Create()
        {
            var products = await _context.Product.Where(p => p.Quantity > 0).ToListAsync();

            OrderCreateViewModel viewModel = new OrderCreateViewModel
            {
                ProductsSelectList = products.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }),
                Products = products
            };

            TempData.Clear();
            return View(viewModel);
        }

        // POST: Order/AddProduct
        // Adiciona um novo produto ao pedido
        [HttpPost]
        public async Task<IActionResult> AddProduct(OrderCreateViewModel viewModel)
        {
            var products = await _context.Product.Where(p => p.Quantity > 0).ToListAsync();

            // Busca de produto pelo ID
            var selectedProduct = products.FirstOrDefault(p => p.Id == viewModel.SelectedProductId);
            if (selectedProduct == null)
                return NotFound("Product not found.");

            // Verifica a quantidade do produto
            if (selectedProduct.Quantity >= viewModel.Quantity)
            {
                TempData[viewModel.SelectedProductId.ToString()] = viewModel.Quantity;
            }
            else
            {
                viewModel.Message = "Insufficient stock.";
            }

            // Calcular preço total do pedido
            viewModel.TotalPrice = TempData.Keys.Sum(key =>
            {
                TempData.Keep(key);
                var product = products.FirstOrDefault(p => p.Id == int.Parse(key));
                var quantity = int.Parse(TempData[key]?.ToString() ?? "0");
                return product?.Price * quantity ?? 0;
            });

            // Atualiza a lista de produtos
            viewModel.ProductsSelectList = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            });

            viewModel.Products = products;

            return View("Create", viewModel);
        }

        // POST: Order/Create
        // Finaliza e salva o pedido 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel viewModel)
        {
            if (!TempData.Keys.Any())
            {
                // Redireciona para index caso não haja produto na lista
                return RedirectToAction(nameof(Index));
            }

            // Criar novo pedido
            var order = new Order
            {
                TimeStamp = DateTime.Now,
                TotalPrice = viewModel.TotalPrice
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Carregar produtos para validação e atualização
            var productIds = TempData.Keys.Select(int.Parse).ToList();
            var products = await _context.Product.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            // Criar itens do pedido
            foreach (var key in TempData.Keys)
            {
                var productId = int.Parse(key);
                var quantity = int.Parse(TempData[key]?.ToString() ?? "0");

                if (!products.TryGetValue(productId, out var product) || product.Quantity < quantity)
                {
                    return BadRequest("Invalid product or insufficient stock.");
                }

                var orderItem = new OrderItem
                {
                    IdOrder = order.Id,
                    IdProduct = productId,
                    Quantity = quantity
                };

                _context.OrderItem.Add(orderItem);

                // Atualizar estoque
                product.Quantity -= quantity;
                _context.Product.Update(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/Edit/5
        // Exibe a edição de um pedido
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Edit/5
        // Salva as alterações que foram editadas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeStamp,TotalPrice")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Order.Any(e => e.Id == order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Order/Details/5
        // Exibe os detalhes de um pedido
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Delete/5
        // Exibe a confirmação para exclusão
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        // Remove um pedido após exclusão
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}