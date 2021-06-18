using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class ProductoSolicitado
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int WorkOrderId { get; set; }
        public int Cantidad { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public DateTime FechaOperacion { get; set; }




        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
