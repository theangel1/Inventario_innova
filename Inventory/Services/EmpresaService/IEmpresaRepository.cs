using Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Services.EmpresaService
{
    public interface IEmpresaRepository
    {
        int GetEmpresaIdByRut(string rut);
        int GetEmpresaIdByUser();
        int GetIdInnovita();

    }
}
