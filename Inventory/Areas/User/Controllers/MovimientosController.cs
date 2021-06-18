using Inventory.Data;
using Inventory.Models;
using Inventory.Models.ViewModel;
using Inventory.Services.EmpresaService;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Inventory.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = (SD.Admin + "," + SD.Control))]
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;
        private readonly IConfiguration configuration;
        private readonly IEmpresaRepository _empresaRepository;        

        [BindProperty]
        public MovementViewModel MovimientoVM { get; set; }

        public MovimientosController(ApplicationDbContext context, HostingEnvironment hostingEnvironment, IConfiguration config,
            IEmpresaRepository empresaRepository)
        {
            _db = context;
            configuration = config;
            _empresaRepository = empresaRepository;
            _hostingEnvironment = hostingEnvironment;            

            MovimientoVM = new MovementViewModel()
            {
                Movimiento = new Movimiento()
            };
        }

        // GET: User/Movimientos
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetMovements()
        {
            try
            {
                var movimientoDb = _db.Movimiento.Include(u => u.ApplicationUser).Where(m => m.EmpresaId == _empresaRepository.GetEmpresaIdByUser());
                var recordsTotal = movimientoDb.Count();
                var data = movimientoDb.ToList();
                return Json(new { recordsFiltered = recordsTotal, data });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public IActionResult IndexCompany()
        {
            ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
            return View();
        }

        public JsonResult GetAllMovements(int idEmpresa)
        {
            try
            {
                var movimientoDb = _db.Movimiento.Include(u => u.ApplicationUser).Where(m => m.EmpresaId == idEmpresa);
                var recordsTotal = movimientoDb.Count();
                var data = movimientoDb.ToList();
                return Json(new { recordsFiltered = recordsTotal, data });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MovementViewModel objMovVM = new MovementViewModel()
            {
                Movimiento = await _db.Movimiento.Include(e => e.Empresa).Include(u => u.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id),
                DetalleMovimiento = new List<MovimientoDetail>()
            };

            List<MovimientoDetail> objDetalle = await _db.MovimientoDetail.Include(p => p.Producto).Where(p => p.MovimientoId == id).ToListAsync();

            foreach (var item in objDetalle)
            {
                objMovVM.DetalleMovimiento.Add(item);
            }
            return View(objMovVM);
        }

        #region Create
        public IActionResult Create()
        {
            ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (ModelState.IsValid)
            {
                MovimientoVM.Movimiento.FechaMovimiento = DateTime.Now;
                MovimientoVM.Movimiento.UserId = currentUserId;
                MovimientoVM.Movimiento.EmpresaId = MovimientoVM.Movimiento.EmpresaId;

                _db.Add(MovimientoVM.Movimiento);

                var MovId = MovimientoVM.Movimiento.Id;

                double saldo = 0;

                for (int i = 0; i < MovimientoVM.DetalleMovimiento.Count; i++)
                {
                    var productsFromDB = _db.Producto.Where(p => p.Id == MovimientoVM.DetalleMovimiento[i].ProductoId
                    && p.EmpresaId == MovimientoVM.Movimiento.EmpresaId).FirstOrDefault();


                    if (productsFromDB != null)
                        MovimientoVM.DetalleMovimiento[i].ProductoId = productsFromDB.Id;

                    /*Traer saldo del detalle aasociado al producto*/

                    var detallSaldoDb = _db.MovimientoDetail.
                        Where(p => p.ProductoId == MovimientoVM.DetalleMovimiento[i].ProductoId).LastOrDefault();

                    if (detallSaldoDb != null)
                    {
                        var detalleProd = detallSaldoDb;

                        saldo = detalleProd.Saldo;
                    }

                    if (MovimientoVM.Movimiento.TipoOperacion == TipoOperacion.Entrada)
                        MovimientoVM.DetalleMovimiento[i].Saldo = saldo + MovimientoVM.DetalleMovimiento[i].Cantidad;
                    else if (MovimientoVM.Movimiento.TipoOperacion == TipoOperacion.Salida)
                        MovimientoVM.DetalleMovimiento[i].Saldo = saldo - MovimientoVM.DetalleMovimiento[i].Cantidad;

                    MovimientoVM.DetalleMovimiento[i].MovimientoId = MovId;
                    _db.MovimientoDetail.Add(MovimientoVM.DetalleMovimiento[i]);


                }


                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
            return View(MovimientoVM);
        }
        #endregion
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _db.Movimiento.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }
            return View(movimiento);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TipoOperacion,Cantidad,Total,FechaMovimiento")] Movimiento movimiento)
        {
            if (id != movimiento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(movimiento);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoExists(movimiento.Id))
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
            return View(movimiento);
        }

        // GET: User/Movimientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _db.Movimiento
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // POST: User/Movimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movimiento = await _db.Movimiento.FindAsync(id);
            _db.Movimiento.Remove(movimiento);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoExists(int id)
        {
            return _db.Movimiento.Any(e => e.Id == id);
        }
        //Extension de controlador

        public IActionResult CargaCompra()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaCompra")]
        public IActionResult CargaCompraPost()
        {

            int empresaId = _empresaRepository.GetEmpresaIdByUser();
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;


            if (files.Count != 0)
            {
                var uploads = Path.Combine(webRootPath, SD.folder);


                using (var filestream = new FileStream(Path.Combine(uploads, files[0].FileName), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                var xmlCompra = webRootPath + @"\" + SD.folder + @"\" + files[0].FileName;

                var doc = XDocument.Load(xmlCompra);


                try
                {
                    /* foreach (var item in doc.Element("EnvioDTE").Element("SetDTE").Element("DTE").Elements("Documento"))
                     {
                         var _movimiento = new Movimiento()
                         {
                             EmpresaId = empresaId,
                             Cantidad = int.Parse(item.Element("QtyItem").Value),

                             // RangoDesde = int.Parse(item.Element("RNG").Element("D").Value),

                         };



                         var empresa = _db.Empresa.Where(e => e.Id == empresaId).FirstOrDefault();

                         if (item.Element("RE").Value != empresa.Rut)
                         {
                             ViewBag.Message = "El folio cargado no corresponde a su empresa";
                             return View();
                         }

                         //Deberia copiar el folio al ambiente del webservice tambien

                     }*/
                }
                catch (Exception)
                {
                    throw;
                }



                return RedirectToAction("Movimientos", new { mensaje = "fuck me" });
            }
            return View();
        }

        public JsonResult GetProductos(int idEmpresa)
        {
            var insumosFromDB = _db.Producto.Where(e => e.EmpresaId == idEmpresa);
            var _TotalRecords = insumosFromDB.Count();
            var data = insumosFromDB.ToList();
            return Json(new { recordsFiltered = _TotalRecords, data });
        }


       

        /// <summary>
        /// Función que guarda la excepcion generada en cierto código en la base de datos.
        /// </summary>
        /// <param name="ex"></param>
      

        #region Procedimiento Almacenado
        public IActionResult ProView()
        {
            return View();
        }
        public JsonResult GetProView()
        {
            var listaSp = new List<JObject>();

            string conString = ConfigurationExtensions.GetConnectionString(configuration, "DefaultConnection");
            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_movimientos", con))
                {                    

                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    cmd.ExecuteNonQuery();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow item in dt.Rows)
                    {
                        JObject objJson = new JObject
                        {
                            {"MovimientoId", item["MovimientoId"].ToString() },
                            { "Nombre", item["Nombre"].ToString() },
                            { "tipoMovimiento", item["tipoMovimiento"].ToString().ToUpper() },
                            {"descripcion", item["descripcion"].ToString()},
                            { "cantidad", item["cantidad"].ToString()},
                            {"sku",item["sku"].ToString()},
                            {"ProductoId", item["ProductoId"].ToString() },
                            {"saldo", item["saldo"].ToString() },
                        };
                        listaSp.Add(objJson);
                    }
                    con.Close();
                }

                var _TotalRecords = listaSp.Count();
                var data = listaSp.ToList();

                return Json(new { recordsFiltered = _TotalRecords, data });
            }



        }
        #endregion
    }
}
