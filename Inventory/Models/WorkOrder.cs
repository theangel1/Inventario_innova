using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }        
        public string UserId { get; set; }

        [DisplayName("Fecha Creación")]
        public DateTime FechaCreacion { get; set; }
        
        
        [Required, DisplayName("Fecha Término")]
        public DateTime FechaTermino { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DisplayName("Estado Orden de Trabajo")]
        public EstadoOT EstadoOT { get; set; }

        [DisplayName("N° Factura Externo") ]
        public int NumeroFacturaExterno { get; set; }

        
        public int EmpresaId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Jornada Jornada { get; set; }
        
        [DisplayName("Retail")]
        public string NombreRetail { get; set; }       
        
        
        [DisplayName("N° Factura Retail")]
        public int NumeroFacturaRetail { get; set; }

        [Required, DisplayName("Orden de Compra")]
        public string OrdenCompra { get; set; }

        public string Comentario { get; set; }

        public bool IsActive { get; set; }       

        public bool IsReassigned { get; set; }
        
        public int? OrdenTrabajoReasignadaID { get; set; }
        public int? CitaID { get; set; }

        /*Foreign Keys*/
        [ForeignKey("UserId"), DisplayName("Nombre Usuario")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("CitaID"), DisplayName("Cita")]
        public virtual Cita Cita { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

    }

    public enum EstadoOT
    { 
        Creado, //Creacion OT        
        Facturado, //se le factura a externo y se resta stock de innovita (add stock a externo)
        Entregado, // externo entrega productos a innova y se le resta a su stock (add stock a innova)
        Pendiente, //la problematica acá, es que de 3 productos, se entreguen solo 2
        Despachar,
        Asignado,//Asignado
        Solicitado, //Cuando el externo solicita stock
        Reasignado,//Reasignado
        Despachado, 
         //estado similar a creado, no hay movimientos de stock
    }

    public enum Jornada
    { 
        AM,PM
    }
}
