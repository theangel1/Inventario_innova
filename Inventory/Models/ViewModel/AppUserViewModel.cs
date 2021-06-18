using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModel
{
    public class AppUserViewModel
    {
        public ApplicationUser User { get; set; }
        public string Rol { get; set; }
        public string RolId { get; set; }
        public IEnumerable<object> RolList { get; set; }
        public IEnumerable<object> EmpresaList { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Contraseña")]
        public string Pass { get; set; }

        [Display(Name = "Confirmación de contraseña")]
        [Compare("Pass", ErrorMessage = "Su contraseña y su confimación de contraseña deben coincidir.")]
        public string ConfPass { get; set; }
    }
}
