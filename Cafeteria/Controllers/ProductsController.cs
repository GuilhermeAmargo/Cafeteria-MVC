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
    public class ProductsController : Controller
    {
        private readonly CafeteriaContext _context;

        public ProductsController(CafeteriaContext context)
        {
            _context = context;
        }

        // GET: Products
        // Lista todos os produtos com filtro de categoriae nome 
        public async Task<IActionResult> Index(string productCategory, string searchString)
        {
            if (_context.Product == null)
            {
                // Retorna erro se for nulo
                return Problem("Entity set 'CafeteriaContext.Product'  is null.");
            }

            // Lista distinta
            IQueryable<string> categoryQuery =  from p in _context.Product
                                                orderby p.Category
                                                select p.Category;

            // Lista de produtos
            var products =  from p in _context.Product
                            select p;

            // Filtra por nomes
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name!.ToUpper().Contains(searchString.ToUpper()));
            }

            // Filtra por categoria
            if (!string.IsNullOrEmpty(productCategory))
            {
                products = products.Where(x => x.Category == productCategory);
            }

            // Retorna a view com os produtos filtrados
            var productCategoryVM = new ProductCategoryViewModel
            {
                Categories = new SelectList(await categoryQuery.Distinct().ToListAsync()),
                Products = await products.ToListAsync()
            };
            
            return View(productCategoryVM);
        }
        // GET: Products/Details/5
        // Exibe detalhes de um produto
        public async Task<IActionResult> Details(int? id)
        {
            // Se id é nulo == ERRO
            if (id == null)
            {
                return NotFound();
            }

            // Busca produto pelo ID
            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        // Retorna a view para a criação de um novo produto
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // Cria um novo produto
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Quantity,Category,Price")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        // Retorna a view para edição de produto
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca produto por ID
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // Atualiza um produto
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Quantity,Category,Price")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            return View(product);
        }

        // GET: Products/Delete/5
        // Retorna a view para exclusão
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca produto pelo ID
            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        // Exclui um produto 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Verifica existência de produto
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
