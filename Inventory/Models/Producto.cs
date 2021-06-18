using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models
{
    public class Producto
    {
        public int Id { get; set; }

        
        public string SKU { get; set; } 

        public string Nombre { get; set; }

        [DisplayName("Precio de Compra")]
        public double? PrecioCompra { get; set; } = 0;

        [DisplayName("Precio de Venta")]
        public double? PrecioVenta { get; set; } = 0;

        [DisplayName("Stock Min.")]
        public double? StockMinimo { get; set; } = 0;

        [DisplayName("Stock Max")]
        public double? StockMaximo { get; set; } = 0;

        [DisplayName("Imagen")]
        public string ImagenUrl { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TipoProducto TipoProducto { get; set; }
        

        public int? EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }



    }

    public enum TipoProducto
    {
        Producto,
        Insumo,
        MateriaPrima,
        Corte,
        [DisplayName("Producto sin kit de corte")]
        ProductoSinKitCorte,
        [DisplayName("Producto terminado")]
        ProductoTerminado
    }
}
