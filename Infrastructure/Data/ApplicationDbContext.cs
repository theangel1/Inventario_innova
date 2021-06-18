using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }        
        public DbSet<Movimiento> Movimiento { get; set; }
        public DbSet<MovimientoDetail> MovimientoDetail { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<WorkOrder> WorkOrder { get; set; }
        public DbSet<WorkOrderDetail> WorkOrderDetail { get; set; }
        public DbSet<Cita> Cita { get; set; }

       





    }
}
