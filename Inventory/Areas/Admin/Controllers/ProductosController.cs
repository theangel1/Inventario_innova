using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Models;
using Inventory.Models.ViewModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting.Internal;
using Inventory.Utility;
using System.IO;
using Inventory.Services.EmpresaService;

namespace Inventory.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductosController : Controller
    {
        private readonly HostingEnvironment _hostingEnvironment;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ProductViewModel ProductosVM { get; set; }

        public ProductosController(ApplicationDbContext db, HostingEnvironment hostingEnvironment, IEmpresaRepository empresaRepository)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            this._empresaRepository = empresaRepository;
            ProductosVM = new ProductViewModel()
            {
                Producto = new Producto()
            };
        }

        
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult LoadProductos(string Type)
        {

            int recordsTotal;
            List<Producto> data;
            IQueryable<Producto> query;

            if ( Type == null)
            {
                Type = "cortes";
            }
            switch (Type.ToLower())
            {
                case "cortes":
                    query = _db.Producto.Where(p => p.TipoProducto == TipoProducto.Corte && p.EmpresaId == _empresaRepository.GetEmpresaIdByUser());
                    recordsTotal = query.Count();
                    data = query.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });

                case "muebles":
                    query = _db.Producto.Where(p => p.TipoProducto == TipoProducto.Producto && p.EmpresaId == _empresaRepository.GetEmpresaIdByUser());
                    recordsTotal = query.Count();
                    data = query.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });

                case "terminados":
                    query = _db.Producto.Where(p => p.TipoProducto == TipoProducto.ProductoTerminado && p.EmpresaId == _empresaRepository.GetEmpresaIdByUser());
                    recordsTotal = query.Count();
                    data = query.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                    
                default:
                    query = _db.Producto.Where(p => p.EmpresaId == _empresaRepository.GetEmpresaIdByUser());
                    recordsTotal = query.Count();
                    data = query.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                    
            }

        }

        // GET: Admin/Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _db.Producto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Admin/Productos/Create
        public IActionResult Create()
        {           
            return View(ProductosVM);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductosVM);
            }

            ProductosVM.Producto.EmpresaId = _empresaRepository.GetEmpresaIdByUser();

            _db.Add(ProductosVM.Producto);
            await _db.SaveChangesAsync();


            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Producto.Find(ProductosVM.Producto.Id);

            if (files.Count != 0)
            {
                //Image has been uploaded
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream = new FileStream(Path.Combine(uploads, ProductosVM.Producto.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                productsFromDb.ImagenUrl = @"\" + SD.ImageFolder + @"\" + ProductosVM.Producto.Id + extension;
            }
            else
            {
              
                productsFromDb.ImagenUrl = null;
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        // GET: Admin/Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductosVM.Producto = await _db.Producto.SingleOrDefaultAsync(m => m.Id == id);

            if (ProductosVM.Producto == null)
            {
                return NotFound();
            }

            return View(ProductosVM);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var productFromDb = _db.Producto.Where(m => m.Id == ProductosVM.Producto.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)
                {
                    //if user uploads a new image
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDb.ImagenUrl);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductosVM.Producto.Id + extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductosVM.Producto.Id + extension_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, ProductosVM.Producto.Id + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    ProductosVM.Producto.ImagenUrl = @"\" + SD.ImageFolder + @"\" + ProductosVM.Producto.Id + extension_new;
                }

                if (ProductosVM.Producto.ImagenUrl != null)
                {
                    productFromDb.ImagenUrl = ProductosVM.Producto.ImagenUrl;
                }

                productFromDb.Nombre = ProductosVM.Producto.Nombre;                
                productFromDb.PrecioCompra = ProductosVM.Producto.PrecioCompra;
                productFromDb.PrecioVenta = ProductosVM.Producto.PrecioVenta;
                productFromDb.StockMinimo = ProductosVM.Producto.StockMinimo;
                productFromDb.StockMaximo = ProductosVM.Producto.StockMaximo;


                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ProductosVM);
        }

        // GET: Admin/Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _db.Producto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Admin/Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _db.Producto.FindAsync(id);
            _db.Producto.Remove(producto);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { Type = "cortes" });
        }

        private bool ProductoExists(int id)
        {
            return _db.Producto.Any(e => e.Id == id);
        }

     
    }
}
