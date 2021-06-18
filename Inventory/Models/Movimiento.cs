using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        public string UserId { get; set; }        
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TipoOperacion TipoOperacion { get; set; }

        public DateTime FechaMovimiento { get; set; }

        public int EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }

    public enum TipoOperacion
    {
        Entrada, Salida
    }
}
