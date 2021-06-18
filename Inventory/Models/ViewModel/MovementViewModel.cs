using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModel
{
    public class MovementViewModel
    {
        public Movimiento Movimiento { get; set; }
        public List<MovimientoDetail> DetalleMovimiento { get; set; }

    }
}
