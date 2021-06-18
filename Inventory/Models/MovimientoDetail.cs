using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class MovimientoDetail
    {
        public int Id { get; set; }
        public int MovimientoId { get; set; }        
        public int ProductoId { get; set; }
        public double Cantidad { get; set; }

        public double Saldo { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("MovimientoId")]
        public virtual Movimiento Movimiento { get; set; }

    }
}
