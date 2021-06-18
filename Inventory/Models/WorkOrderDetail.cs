using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class WorkOrderDetail
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int CantidadNueva { get; set; }
        public int CantidadARecepcionar { get; set; }
        public string Lpn { get; set; }
        public bool IsReadyForDeparture { get; set; }

        
        public string MotivoDevolucion { get; set; }
        

        [NotMapped]
        public string OrdenCompraAux { get; set; }
        [NotMapped]
        public string JornadaAux { get; set; }
        [NotMapped]
        public string EmpresaIDAux { get; set; }
        [NotMapped]
        public DateTime FechaTerminoAux { get; set; }
        [NotMapped]
        public string NombreRetailAux { get; set; }
        [NotMapped]
        public int NumeroFacturaRetailAux { get; set; }
        [NotMapped]
        public string LpnAux { get; set; }
        [NotMapped]
        public string NumeroCitaAux { get; set; }
        [NotMapped]
        public string PatenteCamionAux { get; set; }

        [NotMapped]
        public string HoraSalidaAux { get; set; }
        [NotMapped]
        public string Role { get; set; }


        /*Relaciones*/
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
      
        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }        

    }
}
