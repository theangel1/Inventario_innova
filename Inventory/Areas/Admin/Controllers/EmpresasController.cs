using System;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Inventory.Utility;



namespace Inventory.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Admin)]
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Empresa Empresa { get; set; }
        public EmpresasController(ApplicationDbContext db)
        {
            _db = db;
            Empresa = new Empresa();
        }

        // GET: Admin/Empresas
        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetEmpresas()
        {
            var data = await _db.Empresa.ToListAsync();

            var totalRecords = data.Count();

            return Json(new { recordsFiltered = totalRecords, data });
        }

        // GET: Admin/Empresas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)            
                return NotFound();            

            var empresa = await _db.Empresa
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)            
                return NotFound();            

            return View(empresa);
        }
        
        public IActionResult Create()
        {            
            return View();
        }      
        
        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<IActionResult> CreatepOST()
        {
            if (ModelState.IsValid)
            {              
                /*En esta logica, se copian los productos de la empresa innovita a la nueva*/
                var productosFromdb = _db.Producto.Where(p => p.EmpresaId == 1).ToList();
              
                var _empresa = new Empresa()
                {
                    Nombre = Empresa.Nombre.Trim().ToUpper(),
                    Rut = Empresa.Rut.Trim().ToUpper(),
                };

                _db.Add(_empresa);

                

                
                

                for (int i = 0; i < productosFromdb.Count(); i++)
                {
                    var _prodocto = new Producto()
                    { 
                        Nombre = productosFromdb[i].Nombre,                        
                        PrecioVenta = productosFromdb[i].PrecioVenta,
                        SKU = productosFromdb[i].SKU,
                        //Categoria = productosFromdb[i].Categoria,
                        TipoProducto = productosFromdb[i].TipoProducto,                        
                        ImagenUrl = productosFromdb[i].ImagenUrl,
                        EmpresaId = _empresa.Id

                    };
                    _db.Add(_prodocto);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { serverMessage = "La empresa se ha creado exitosamente junto con sus productos." });
            }
            return View(Empresa);
        }

        // GET: Admin/Empresas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empresa = await _db.Empresa.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }
            return View(empresa);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Rut")] Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(empresa);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpresaExists(empresa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
        }

        // GET: Admin/Empresas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empresa = await _db.Empresa
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            return View(empresa);
        }

        // POST: Admin/Empresas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresa = await _db.Empresa.FindAsync(id);
            try
            {
                var productoEmpresa = _db.Producto.Where(p => p.EmpresaId == empresa.Id).ToList();
                
                spDeleteProducto(empresa.Id);
             
                    _db.Remove(empresa);
                    _db.SaveChanges();
                            }
            catch (Exception ex)
            {
                ViewBag.serverError = "Se están eliminando productos, reintente eliminar la empresa.";
                return View(empresa);
            }
            
          

            return RedirectToAction(nameof(Index));
        }

        private void spDeleteProducto(int id)
        {
            _db.Database.ExecuteSqlCommand("EXECUTE dbo.sp_delete_productos {0}", id);            
        }

        private bool EmpresaExists(int id)
        {
            return _db.Empresa.Any(e => e.Id == id);
        }
    }
}
