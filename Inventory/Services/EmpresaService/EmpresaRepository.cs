
using Inventory.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace Inventory.Services.EmpresaService
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmpresaRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetEmpresaIdByRut(string rut)
        {
            var empresa = _context.Empresa.Where(e => e.Rut == rut.ToUpper().Trim());

            if (empresa.Count() < 1)
            {
                return 0;
            }
            return empresa.Select(e => e.Id).FirstOrDefault();
        }

        public int GetEmpresaIdByUser()
        {

            var user = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = _context.ApplicationUser.Where(e => e.Id == user).FirstOrDefault();

            return _context.Empresa.Where(e => e.Id == usuario.EmpresaId).Select(e => e.Id).First();
        }

        public int GetIdInnovita()
        {
            return _context.Empresa.Where(e => e.Rut == "76929437-6").Select(e => e.Id).First();
        }
    }
}
