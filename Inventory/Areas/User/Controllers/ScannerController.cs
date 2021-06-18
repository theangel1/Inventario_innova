using Inventory.Data;
using Inventory.Models;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Control + "," + SD.Admin)]
    public class ScannerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ScannerController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ValidateLPN(string lpn)
        {
            string mensaje = string.Empty;

            //Formeateo el lpn que viene por parametro.

            if (string.IsNullOrEmpty(lpn))
            {
                mensaje = "Lpn vacío, debe escanear código de barras.";
                return Json(mensaje);
            }

            string _lpn = lpn.ToUpper().Trim();

            var detalleFromdb = _db.WorkOrderDetail.Include(p => p.Producto).Where(w => w.Lpn == _lpn).ToList();

            if (detalleFromdb.Count == 0)
            {
                mensaje = "No existe ninguna orden de trabajo para el LPN " + _lpn;
                return Json(mensaje);
            }
           
                for (int i = 0; i < detalleFromdb.Count; i++)
                {
                    var ordenTrabajo = _db.WorkOrder.Where(a => a.Id == detalleFromdb[i].WorkOrderId).FirstOrDefault();

                    if (detalleFromdb[i].Lpn == _lpn && ordenTrabajo.EstadoOT == EstadoOT.Despachado)
                    {
                        /*retornamos id para ir a recepcionar el producto. El js nos manda a workorders/recepcion*/
                        return Json(detalleFromdb[i].Id);
                    }
                    else if (ordenTrabajo.EstadoOT != EstadoOT.Entregado)
                    {
                        return Json($"La orden de trabajo {ordenTrabajo.Id} no se encuentra ENTREGADA");
                    }
                    else if (detalleFromdb[i].Lpn == _lpn && ordenTrabajo.EstadoOT == EstadoOT.Entregado)
                    {
                        detalleFromdb[i].IsReadyForDeparture = true;
                        mensaje = "Orden de trabajo actualizada, debe ingresar " + detalleFromdb[i].Producto.Nombre + " al camión.";

                        if (await ClosePost(ordenTrabajo.Id))
                        {
                            _db.SaveChanges();
                            return Json(mensaje);
                        }
                        else
                        {
                            mensaje = "Ocurrió un error al actualizar una orden de trabajo.";
                            return Json(mensaje);
                        }
                    }

                }
            
            return Json(mensaje);
        }
        public async Task<bool> ClosePost(int id)
        {

            // cambiar a finalizado el estado.
            var innovaId = await _db.Empresa.Where(e => e.Rut == "76399752-9").Select(d => d.Id).FirstOrDefaultAsync();

            var workOrderDB = await _db.WorkOrder.Where(w => w.Id == id).FirstOrDefaultAsync();

            var detalleWorkOrderDB = await _db.WorkOrderDetail.Include(p => p.Producto).Where(m => m.WorkOrderId == id).ToListAsync();         

            for (int i = 0; i < detalleWorkOrderDB.Count; i++)
            {    

                var detalleOtFromDB = _db.WorkOrderDetail.Where(d => d.WorkOrderId == id).ToList();                
                

                if (detalleOtFromDB.Select(a => a.IsReadyForDeparture).LastOrDefault() == detalleOtFromDB[i].IsReadyForDeparture)
                {
                    workOrderDB.EstadoOT = EstadoOT.Despachar;
                }

                await _db.SaveChangesAsync();
                return true;
                
            }
            return false;

        }

        

      
    }
}