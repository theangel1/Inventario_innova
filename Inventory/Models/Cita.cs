using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public string NumeroCita { get; set; }        
        public string Patente { get; set; }
        public string HoraSalidaCamion { get; set; }
    }
}
