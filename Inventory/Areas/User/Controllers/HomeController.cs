using Inventory.Data;
using Inventory.Models;
using Inventory.Services.EmpresaService;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace Inventory.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmpresaRepository _empresaRepository;

        public HomeController(ApplicationDbContext db, IEmpresaRepository empresaRepository)
        {
            _db = db;
            _empresaRepository = empresaRepository;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        public JsonResult LoadWorkOrders()
        {
            try
            {
                if (User.IsInRole(SD.Control) || User.IsInRole(SD.Admin))
                {
                    var otFromdb = _db.WorkOrder.Include(u => u.User).Where(w => w.EstadoOT != EstadoOT.Despachado);
                    var recordsTotal = otFromdb.Count();
                    var data = otFromdb.ToList();

                    return Json(new { recordsFiltered = recordsTotal, data });

                }
                else if (User.IsInRole(SD.Externo))
                {
                    var localOt = _db.WorkOrder.Include(u => u.User).Where(w => w.EmpresaId == _empresaRepository.GetEmpresaIdByUser() && w.EstadoOT != EstadoOT.Despachado);
                    var recordsTotal = localOt.Count();
                    var data = localOt.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                }
                else
                {
                    return Json("Sin OT");
                }
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        [Authorize(Roles = SD.Externo + "," + SD.Admin + "," + SD.Control)]
        public IActionResult Stock()
        {
            return View();
        }
        public JsonResult GetStock()
        {
            try
            {
                var lista = new List<MovimientoDetail>();
                var _productosFromdb = _db.Producto.Where(e => e.EmpresaId == _empresaRepository.GetEmpresaIdByUser()).ToList();
                var _listaMovimientos = _db.MovimientoDetail.ToList();
                for (int i = 0; i < _listaMovimientos.Count; i++)
                {
                    foreach (var item in _productosFromdb)
                    {
                        if (_listaMovimientos[i].ProductoId == item.Id)
                        {
                            var query = _db.MovimientoDetail.Include(p => p.Producto).
                                Where(d => d.ProductoId == item.Id).
                                OrderByDescending(x => x.MovimientoId).FirstOrDefault();

                            if (query != null)
                            {
                                lista.Add(query);
                            }
                        }
                    }

                }

                /*Deberia buscar por duplicados y eliminarlos... o no?*/
                var listaSinDuplicado = lista.Distinct().ToList();
                var _TotalRecords = listaSinDuplicado.Count();
                var data = listaSinDuplicado.ToList();
                return Json(new { recordsFiltered = _TotalRecords, data });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }


        [Authorize(Roles = SD.Admin + "," + SD.Control)]
        public IActionResult StockGeneral()
        {
            ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
            return View();
        }

        public JsonResult GetStockGeneral(int idEmpresa)
        {
            try
            {
                var lista = new List<MovimientoDetail>();

                var _productosFromdb = _db.Producto.Where(e => e.EmpresaId == idEmpresa).ToList();

                var _listaMovimientos = _db.MovimientoDetail.ToList();

                for (int i = 0; i < _listaMovimientos.Count; i++)
                {
                    foreach (var item in _productosFromdb)
                    {
                        if (_listaMovimientos[i].ProductoId == item.Id)
                        {
                            var query = _db.MovimientoDetail.Include(p => p.Producto).
                                Where(d => d.ProductoId == item.Id).
                                OrderByDescending(x => x.MovimientoId).FirstOrDefault();

                            if (query != null)
                                lista.Add(query);
                        }
                    }
                }

                /*Deberia buscar por duplicados y eliminarlos... o no?*/
                var listaSinDuplicado = lista.Distinct().ToList();
                var _TotalRecords = listaSinDuplicado.Count();
                var data = listaSinDuplicado.ToList();
                return Json(new { recordsFiltered = _TotalRecords, data });

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

     

        public IActionResult Produccion()
        {
            return View();
        }

        public JsonResult GetProduccion()
        {
            try
            {                
                var _produccionFromdb = _db.WorkOrderDetail.Include(w => w.WorkOrder).Include(w => w.WorkOrder.Cita).
                    Include(e => e.WorkOrder.Empresa).Include(p => p.Producto).OrderBy(e => e.WorkOrder.EstadoOT).
                    Where(c => c.WorkOrder.Cita.NumeroCita != null  &&
                    c.WorkOrder.EstadoOT != EstadoOT.Creado &&
                    c.WorkOrder.EstadoOT != EstadoOT.Asignado &&
                    c.WorkOrder.EstadoOT != EstadoOT.Despachado
                    );

                var recordsTotal = _produccionFromdb.Count();

                var data = _produccionFromdb.ToList();

                return Json(new { recordsFiltered = recordsTotal, data });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
     
        /* JSON ACTIONS */
        public string GetEmpresa()
        {
            var user = _db.ApplicationUser.FirstOrDefault(i => i.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var emp = _db.Empresa.FirstOrDefault(i => i.Id == user.EmpresaId);
            var finalString = emp.Nombre;
            return finalString;
        }
    }
}
