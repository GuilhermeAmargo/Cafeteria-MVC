using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cafeteria.Models;

namespace Cafeteria.Data
{
    public class CafeteriaContext : DbContext
    {
        public CafeteriaContext(DbContextOptions<CafeteriaContext> options)
            : base(options)
        {
        }

        // Tabela de produtos
        public DbSet<Product> Product { get; set; } = default!;

        // Tabela de pedidos
        public DbSet<Order> Order { get; set; } = default!;
        
        // Tabela itens pedidos 
        public DbSet<OrderItem> OrderItem { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração de OrderItem -> Order
            modelBuilder.Entity<OrderItem>()
                .HasOne<Order>()
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.IdOrder)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração de OrderItem -> Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.IdProduct);

            base.OnModelCreating(modelBuilder);
        }
    }
}