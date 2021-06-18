using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Nombre { get; set; }       

        public int EmpresaId { get; set; }


        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }
    }
}
