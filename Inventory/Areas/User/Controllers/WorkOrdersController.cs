using Inventory.Data;
using Inventory.Models;
using Inventory.Models.ViewModel;
using Inventory.Services.EmpresaService;
using Inventory.Services.WorkOrderService;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Areas.User.Controllers
{
    [Area("User")]
    public class WorkOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IWorkOrderService _workOrderService;

        [BindProperty]
        public WorkOrderViewModel WorkOrderVM { get; set; }


        public WorkOrdersController(ApplicationDbContext db, HostingEnvironment hostingEnvironment, 
            IEmpresaRepository empresaRepository, IWorkOrderService workOrderService)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            _empresaRepository = empresaRepository;
            _workOrderService = workOrderService;

            WorkOrderVM = new WorkOrderViewModel()
            {
                WorkOrder = new WorkOrder()
            };
        }

        public async Task<IActionResult> UpdateWorkOrder(WorkOrderViewModel updateWorkOrder)
        {

            ServiceResponse<WorkOrder> response = await _workOrderService.UpdateWorkOrder(updateWorkOrder.WorkOrder);

            if ( response.Data == null)
            {                
                TempData["updated"] = response;
                return RedirectToAction("Edit", new { id = updateWorkOrder.WorkOrder.Id });
            }

           TempData["updated"] = "Datos actualizados";
            return RedirectToAction("Edit", new { id = updateWorkOrder.WorkOrder.Id });
        }


        #region Index
        [Authorize(Roles = SD.Control + "," + SD.Admin + "," + SD.Externo)]
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult LoadDataIndex()
        {
            try
            {
                /*Si el usuario es externo, verá solo sus ots.*/
                if (User.IsInRole(SD.Externo))
                {
                    var otFromDb = _db.WorkOrder.Include(e => e.Empresa).Where(p => p.EmpresaId == _empresaRepository.GetEmpresaIdByUser()
                    && p.EstadoOT != EstadoOT.Despachado);
                    var recordsTotal = otFromDb.Count();
                    var data = otFromDb.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                }
                else
                {
                    var otFromDb = _db.WorkOrder.Include(e => e.Empresa).Where(o => o.EstadoOT != EstadoOT.Despachado);
                    var recordsTotal = otFromDb.Count();
                    var data = otFromDb.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                }
            }
            catch (Exception ex)
            {

                return Json(ex);
            }
        }


        #endregion
        #region Detalles
        [Authorize(Roles = SD.Control + "," + SD.Admin + "," + SD.Externo)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).Include(l => l.Cita).FirstOrDefaultAsync(m => m.Id == id);
            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == id).ToListAsync();
            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }
            return View(WorkOrderVM);

        }

        [HttpPost, ActionName("Details"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsPost(int id)
        {
            var _workOrdersFromdb = await _db.WorkOrder.Include(e => e.Empresa).Where(w => w.Id == id).FirstOrDefaultAsync();
            var _workOrdersDetailFromdb = await _db.WorkOrderDetail.Include(p => p.Producto).Where(d => d.WorkOrderId == id).ToListAsync();
            string mensaje = string.Empty;

            if (_workOrdersFromdb.EstadoOT == EstadoOT.Creado || _workOrdersFromdb.EstadoOT == EstadoOT.Asignado)
            {

                for (int i = 0; i < _workOrdersDetailFromdb.Count; i++)
                {
                    /*Modificacion numero 5: "No dejar solicitar productos menos de lo necesario al externo"*/

                    var productoExt = _db.Producto.
                        Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU &&
                        p.EmpresaId == _workOrdersFromdb.EmpresaId).FirstOrDefault();

                    var stock = GetSaldoProducto(_workOrdersFromdb.EmpresaId, productoExt);
                    var cantidadResta = stock - _workOrdersDetailFromdb[i].Cantidad;


                    if (WorkOrderVM.DetalleOT[i].CantidadNueva > cantidadResta && cantidadResta > 0)
                    {
                        mensaje = "No puede solicitar una cantidad superior a su stock.";
                    }
                    else if (WorkOrderVM.DetalleOT[i].CantidadNueva > WorkOrderVM.DetalleOT[i].Cantidad)
                    {
                        mensaje = "No puede solicitar una cantidad mayor a la estipulada en la orden de trabajo.\n";
                    }
                    else if (WorkOrderVM.DetalleOT[i].CantidadNueva == 0 && stock < 1)
                    {
                        mensaje = "Stock insuficiente para continuar la operación. Stock: " + stock;

                    }
                    else if (WorkOrderVM.DetalleOT[i].CantidadNueva == 0 && stock > 0)
                    {
                        mensaje = "Orden de trabajo actualizada a estado Facturado.";
                        MovimientoSalidaConSaldo(i);
                        _workOrdersFromdb.EstadoOT = EstadoOT.Facturado;
                        EntradaStockExterno();
                        await _db.SaveChangesAsync();
                    }
                    else if (WorkOrderVM.DetalleOT[i].CantidadNueva < cantidadResta)
                    {
                        mensaje = "El externo dispone de suficiente stock para esta orden de trabajo.";
                    }
                    else if (stock < 1 && WorkOrderVM.DetalleOT[i].CantidadNueva != WorkOrderVM.DetalleOT[i].Cantidad)
                    {
                        mensaje = "No puede solicitar una cantidad menor a la establecida en la orden de trabajo y con stock menor a 0.";
                    }
                    else
                    {
                        _workOrdersDetailFromdb[i].CantidadNueva = WorkOrderVM.DetalleOT[i].CantidadNueva;
                        _workOrdersFromdb.EstadoOT = EstadoOT.Solicitado;
                        mensaje = "Orden de trabajo Actualizada";

                        await _db.SaveChangesAsync();
                        return RedirectToAction("Index", "WorkOrders", new { recepcion = mensaje });
                    }
                }


                ViewBag.Message = mensaje;
            }
            else
            {
                ViewBag.Message = "No se puede actualizar la orden de trabajo actual. Razón: Estado es distinto a creado";
            }

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).FirstOrDefaultAsync(m => m.Id == id);
            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == id).ToListAsync();
            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }
            return View(WorkOrderVM);

        }


        #endregion
        #region Recepcion
        public async Task<IActionResult> Recepcion(int? id)
        {
            if (id == null)
                return NotFound();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.Id == id).FirstOrDefaultAsync();

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).FirstOrDefaultAsync(m => m.Id == objDetalle.WorkOrderId);

            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            WorkOrderVM.DetalleOT.Add(objDetalle);

            return View(WorkOrderVM);
        }
        [HttpPost, ActionName("Recepcion"), ValidateAntiForgeryToken]
        public IActionResult RecepcionPOST()
        {
            int id = WorkOrderVM.WorkOrder.Id;

            var ordenDeTrabajo = _db.WorkOrder.Where(w => w.Id == id).FirstOrDefault();

            var detallesOrdenTrabajo = _db.WorkOrderDetail.Include(p => p.Producto).
                       Where(d => d.ProductoId == WorkOrderVM.DetalleOT[0].ProductoId && d.WorkOrderId == id).ToList();
            //falta if de ot si es que está activa o no

            string mensaje = string.Empty;

            if (ordenDeTrabajo.IsActive)
            {

                for (int i = 0; i < detallesOrdenTrabajo.Count; i++)
                {
                    detallesOrdenTrabajo[i].MotivoDevolucion = WorkOrderVM.DetalleOT[i].MotivoDevolucion;
                }

                ViewBag.Message = mensaje;
                _db.SaveChanges();
                return RedirectToAction("Index", "WorkOrders", new { recepcion = "Se recepcionó " + detallesOrdenTrabajo[0].Producto.Nombre + " de forma éxitosa." });
            }
            else
            {
                ViewBag.recepcion = "Orden de trabajo no se encuentra activa.";
                return View(WorkOrderVM);
            }

        }
        #endregion
        #region Create
        [Authorize(Roles = SD.Control + "," + SD.Admin)]
        public IActionResult Create()
        {
            ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Create")]
        public async Task<IActionResult> CreatePOST()
        {
            /*Falta validar stock*/
            try
            {
                if (ModelState.IsValid)
                {

                    WorkOrderVM.WorkOrder.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    WorkOrderVM.WorkOrder.FechaCreacion = DateTime.Now;

                    if (WorkOrderVM.WorkOrder.EmpresaId == 1)
                        WorkOrderVM.WorkOrder.EstadoOT = EstadoOT.Entregado;
                    else
                        WorkOrderVM.WorkOrder.EstadoOT = EstadoOT.Creado;

                    WorkOrderVM.WorkOrder.IsActive = true;

                    var myfecha = WorkOrderVM.WorkOrder.FechaTermino;
                    _db.Add(WorkOrderVM.WorkOrder);

                    var OTId = WorkOrderVM.WorkOrder.Id;

                    for (int i = 0; i < WorkOrderVM.DetalleOT.Count; i++)
                    {
                        var productsFromDB = _db.Producto.
                            Where(p => p.Id == WorkOrderVM.DetalleOT[i].ProductoId && p.EmpresaId ==
                          _empresaRepository.GetIdInnovita()).FirstOrDefault();

                        if (productsFromDB != null)
                            WorkOrderVM.DetalleOT[i].ProductoId = productsFromDB.Id;

                        WorkOrderVM.DetalleOT[i].WorkOrderId = OTId;
                        _db.WorkOrderDetail.Add(WorkOrderVM.DetalleOT[i]);
                    }

                    await _db.SaveChangesAsync();
                    TempData["alert"] = "Orden de Trabajo creada con éxito.";

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["EmpresaId"] = new SelectList(_db.Empresa, "Id", "Nombre");
                    return View(WorkOrderVM);
                }
            }
            catch (Exception ex)
            {
                ViewData["invalidModel"] = ex.Message.ToString();
                return View(WorkOrderVM);
            }
        }
        #endregion
        #region Edit
        [Authorize(Roles = SD.Control + "," + SD.Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(e => e.Empresa).Include(u => u.User).FirstOrDefaultAsync(f => f.Id == id);

            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == id).ToListAsync();
            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }

            var listaDetalle = new List<WorkOrderDetail>();
            var woDetailFromdb = _db.WorkOrderDetail.Include(w => w.WorkOrder.Empresa).Include(p => p.Producto).ToList();



            if (objDetalle.Count == 1)
            {
                //RELACION 1 A 1

                for (int i = 0; i < woDetailFromdb.Count; i++)
                {

                    //asumis que siempre será 1:1
                    if (woDetailFromdb[i].Producto.SKU == objDetalle[0].Producto.SKU &&
                        woDetailFromdb[i].Id != objDetalle[0].Id &&
                        woDetailFromdb[i].WorkOrder.EmpresaId != objDetalle[0].WorkOrder.EmpresaId &&
                        (woDetailFromdb[i].WorkOrder.EstadoOT == EstadoOT.Facturado ||
                        woDetailFromdb[i].WorkOrder.EstadoOT == EstadoOT.Pendiente) &&
                        woDetailFromdb[i].WorkOrder.IsActive == true &&
                        woDetailFromdb[i].WorkOrder.IsReassigned == false)
                    {
                        //query para contabilizar las lineas de la orden de trabajo y poder agregar de forma correcta la lista.
                        var query = _db.WorkOrderDetail.Where(x => x.WorkOrderId == woDetailFromdb[i].WorkOrderId).ToList();
                        if (query.Count == 1)
                            listaDetalle.Add(woDetailFromdb[i]);
                    }
                }

                WorkOrderVM.WorkOrdersList = from s in listaDetalle
                                             select new
                                             {
                                                 woId = s.WorkOrderId,
                                                 Empresa = s.WorkOrder.Empresa.Nombre,
                                                 oc = s.WorkOrder.OrdenCompra
                                             };


                if (WorkOrderVM.WorkOrdersList.Count() < 1 && WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Facturado ||
                    WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Pendiente)
                {
                    TempData["alert"] += "\tSi la cantidad de ordenes de trabajo mostradas son menores a las que tiene en sistema, " +
                        "puede que la empresa " +
                        "externa tenga productos listos, pero el estado no corresponda a Facturado o pendiente.";
                }
            }
            else
            {
                TempData["alert"] += "\tNo se encontraron ordenes de trabajo que contengan la relacion 1:1 con el producto y la linea de detalle";
            }

            return View(WorkOrderVM);

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            if (id != WorkOrderVM.WorkOrder.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var otFromdb = await _db.WorkOrder.Include(e => e.Empresa).Where(a => a.Id == WorkOrderVM.WorkOrder.Id).FirstOrDefaultAsync();

                if (otFromdb != null)
                {
                    otFromdb.FechaTermino = WorkOrderVM.WorkOrder.FechaTermino;
                    otFromdb.EstadoOT = WorkOrderVM.WorkOrder.EstadoOT;
                    otFromdb.Jornada = WorkOrderVM.WorkOrder.Jornada;
                    otFromdb.NumeroFacturaExterno = WorkOrderVM.WorkOrder.NumeroFacturaExterno;
                    otFromdb.NombreRetail = WorkOrderVM.WorkOrder.NombreRetail;
                    otFromdb.OrdenCompra = WorkOrderVM.WorkOrder.OrdenCompra;
                    otFromdb.Comentario = WorkOrderVM.WorkOrder.Comentario;

                    #region ESTADO.FACTURADO
                    /*Si el estado es facturado, restaremos el stock de corte a innovita/externo y le
                     agregaremos el mueble al externo.*/

                    int _empresaID = 0;

                    if (WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Facturado ||
                        WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Reasignado)
                    {
                        #region Logica Salida Stock    

                        for (int i = 0; i < WorkOrderVM.DetalleOT.Count; i++)
                        {

                            /*Si el externo solicita todos los productos, entonces haremos los movimientos correspondientes*/

                            if (WorkOrderVM.DetalleOT[i].Cantidad == WorkOrderVM.DetalleOT[i].CantidadNueva)
                            {

                                /*Ultima falla*/
                                _empresaID = _empresaRepository.GetIdInnovita();
                                int movId = MovimientoSalida(_empresaID);
                                /*Busqueda de productos para no generar duplicados*/
                                var productoSKUBodega = _db.Producto.Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU
                                && p.EmpresaId == _empresaID).FirstOrDefault();

                                double _saldo = GetSaldoProducto(_empresaID, productoSKUBodega);

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = WorkOrderVM.DetalleOT[i].Cantidad,
                                    ProductoId = productoSKUBodega.Id,
                                    MovimientoId = movId,
                                    Saldo = _saldo - WorkOrderVM.DetalleOT[i].Cantidad
                                });

                                var detalleOtFromDB = _db.WorkOrderDetail.Where
                                    (d => d.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToList();
                                detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
                                detalleOtFromDB[i].Cantidad = WorkOrderVM.DetalleOT[i].Cantidad;
                                EntradaStockExterno();
                            }
                            /*si el externo no solicita, entonces simplemente se le descuente de su stock*/
                            else if (WorkOrderVM.DetalleOT[i].CantidadNueva == 0)
                            {
                                /*debo ver que tenga stock el externo*/
                                var productoSKUBodega = _db.Producto.Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU
                             && p.EmpresaId == WorkOrderVM.WorkOrder.EmpresaId).FirstOrDefault();

                                double _saldo = GetSaldoProducto(WorkOrderVM.WorkOrder.EmpresaId, productoSKUBodega);

                                if (_saldo == 0)
                                {
                                    ViewBag.serverError = otFromdb.Empresa.Nombre + " no posee saldo para el producto " + WorkOrderVM.DetalleOT[i].Producto.Nombre + ". Primero debe solicitar stock. ";
                                    return View(WorkOrderVM);
                                }

                                _empresaID = MovimientoSalidaConSaldo(i);
                            }
                            /*si el externo solicita una cantidad menor a la que tiene en la ot.*/
                            else if (WorkOrderVM.DetalleOT[i].CantidadNueva < WorkOrderVM.DetalleOT[i].Cantidad)
                            {
                                _empresaID = _empresaRepository.GetIdInnovita();

                                int movId = MovimientoSalida(_empresaID);

                                var productoSKUBodega = _db.Producto.Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU &&
                               p.EmpresaId == _empresaID).FirstOrDefault();


                                var detallSaldoDb = _db.MovimientoDetail.Include(x => x.Movimiento).Where
                                    (p => p.ProductoId == WorkOrderVM.DetalleOT[i].ProductoId &&
                                    p.Movimiento.EmpresaId == _empresaID).LastOrDefault();

                                if (detallSaldoDb == null)
                                {
                                    ViewBag.serverError = "Error al recuperar el saldo.";
                                    return View(WorkOrderVM);
                                }

                                double _saldo = 0;

                                if (detallSaldoDb != null)
                                    _saldo = detallSaldoDb.Saldo;

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = WorkOrderVM.DetalleOT[i].CantidadNueva,
                                    ProductoId = productoSKUBodega.Id,
                                    MovimientoId = movId,
                                    Saldo = _saldo - WorkOrderVM.DetalleOT[i].CantidadNueva
                                });

                                /*cantidad restante sale de bodega de externo*/

                                _empresaID = WorkOrderVM.WorkOrder.EmpresaId;

                                int idMovExterno = MovimientoSalida(_empresaID);

                                var productoSKUBodegaExterno = _db.Producto.Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU &&
                               p.EmpresaId == _empresaID).FirstOrDefault();


                                _saldo = GetSaldoProducto(_empresaID, productoSKUBodegaExterno);

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = WorkOrderVM.DetalleOT[i].Cantidad - WorkOrderVM.DetalleOT[i].CantidadNueva,
                                    ProductoId = productoSKUBodegaExterno.Id,
                                    MovimientoId = idMovExterno,
                                    Saldo = _saldo - (WorkOrderVM.DetalleOT[i].Cantidad - WorkOrderVM.DetalleOT[i].CantidadNueva)
                                });

                                var detalleOtFromDB = _db.WorkOrderDetail.Where
                                    (d => d.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToList();
                                detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
                                detalleOtFromDB[i].Cantidad = WorkOrderVM.DetalleOT[i].Cantidad;
                                EntradaStockExterno();
                            }
                        }
                        #endregion

                    }
                    #endregion

                    #region ESTADO.ENTREGADO
                    /*Si el estado es entregado, restaremos el stock de muebles al externo y 
                     le agregaremos stock a INNOVA*/
                    if (WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Entregado || WorkOrderVM.WorkOrder.EstadoOT == EstadoOT.Pendiente)
                    {
                        var detalleOtFromDB = _db.WorkOrderDetail.Where(d => d.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToList();

                        //Validación
                        for (int d = 0; d < WorkOrderVM.DetalleOT.Count; d++)
                        {
                            if (WorkOrderVM.DetalleOT[d].Cantidad > detalleOtFromDB[d].Cantidad)
                            {
                                TempData["serverAlert"] = "No puede entregar el producto debido a que la cantidad ingresada " +
                                    "es mayor a la original.";
                                return RedirectToAction("Edit", new { id = WorkOrderVM.WorkOrder.Id });
                            }
                        }


                        var empresaFromdb = _db.Empresa.Where(e => e.Id == WorkOrderVM.WorkOrder.EmpresaId).FirstOrDefault();
                        var movId = MovimientoSalida(empresaFromdb.Id);

                        /*Logica Si la cantidad que viene en la vista es menor a la cantidad en db,
                        crearemos una nueva orden de trabajo. No puedo meterla dentro de una for...*/

                        WorkOrder _otPendiente = new WorkOrder();
                        bool deleteOtPendiente = true;

                        #region Logica salida stock EXTERNO
                        if (WorkOrderVM.DetalleOT.Count > 0)
                        {
                            _otPendiente.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                            _otPendiente.FechaCreacion = DateTime.Now;
                            _otPendiente.FechaTermino = WorkOrderVM.WorkOrder.FechaTermino.AddDays(1);
                            _otPendiente.EstadoOT = EstadoOT.Pendiente;
                            _otPendiente.NumeroFacturaExterno = WorkOrderVM.WorkOrder.NumeroFacturaExterno;
                            _otPendiente.EmpresaId = WorkOrderVM.WorkOrder.EmpresaId;
                            _otPendiente.Jornada = WorkOrderVM.WorkOrder.Jornada;
                            _otPendiente.NombreRetail = WorkOrderVM.WorkOrder.NombreRetail;
                            _otPendiente.OrdenCompra = WorkOrderVM.WorkOrder.OrdenCompra;
                            _otPendiente.IsActive = true;

                            await _db.WorkOrder.AddAsync(_otPendiente);

                            var idOtPendiente = _otPendiente.Id;

                            for (int i = 0; i < WorkOrderVM.DetalleOT.Count; i++)
                            {
                                //Si me entrega con cantidad 0 GG?
                                if (WorkOrderVM.DetalleOT[i].Cantidad == 0)
                                {
                                    TempData["serverAlert"] = "La cantidad debe ser distinta a 0";
                                    return RedirectToAction("Edit", new { id = WorkOrderVM.WorkOrder.Id });
                                }

                                var producto = _db.Producto.
                                    Where(p => p.Id == WorkOrderVM.DetalleOT[i].ProductoId).FirstOrDefault();

                                //var skuSplit = producto.SKU.Split("-");
                                /*Busqueda de productos para no generar duplicados*/
                                var productoOutExt = _db.Producto.
                                    Where(p => p.SKU == producto.SKU &&
                                    p.EmpresaId == empresaFromdb.Id).FirstOrDefault();

                                double _saldo = GetSaldoProducto(empresaFromdb.Id, productoOutExt);

                                var objDetalleMovimiento = new MovimientoDetail
                                {
                                    Cantidad = WorkOrderVM.DetalleOT[i].Cantidad,
                                    ProductoId = productoOutExt.Id,
                                    MovimientoId = movId
                                };

                                objDetalleMovimiento.Saldo = _saldo - objDetalleMovimiento.Cantidad;
                                _db.MovimientoDetail.Add(objDetalleMovimiento);

                                detalleOtFromDB[i].ProductoId = productoOutExt.Id;

                                /*Si se desea entregar una cantidad mayor, no debe permitirse*/

                                /*Si la cantidad que viene en la vista es menor a la cantidad en db,
                                 crearemos una nueva orden de trabajo....*/
                                //Ojo que no hare el switch de SKU ya que si quedara pendiente, debe estar en su forma 
                                //original de corte.
                                if (WorkOrderVM.DetalleOT[i].Cantidad < detalleOtFromDB[i].Cantidad)
                                {
                                    var detalleOTPendiente = new WorkOrderDetail()
                                    {
                                        WorkOrderId = idOtPendiente,
                                        Cantidad = detalleOtFromDB[i].Cantidad - WorkOrderVM.DetalleOT[i].Cantidad,
                                        ProductoId = WorkOrderVM.DetalleOT[i].ProductoId
                                    };
                                    await _db.AddAsync(detalleOTPendiente);
                                }


                                if (WorkOrderVM.DetalleOT[i].Cantidad != detalleOtFromDB[i].Cantidad)
                                    deleteOtPendiente = false;

                                detalleOtFromDB[i].Cantidad = WorkOrderVM.DetalleOT[i].Cantidad;
                            }
                            if (deleteOtPendiente)
                                _db.Remove(_otPendiente);

                        }
                        #endregion

                        /*agregar nuevo movimiento de stock para INNOVA. Ojo con sku*/

                        var innovaId = await _db.Empresa.Where(e => e.Rut == "76399752-9").Select(d => d.Id).FirstOrDefaultAsync();

                        var movIdInnova = GetIdMovimientoEntrada(innovaId);


                        if (WorkOrderVM.DetalleOT.Count > 0)
                        {
                            for (int x = 0; x < WorkOrderVM.DetalleOT.Count; x++)
                            {
                                /*Busqueda de productos para no generar duplicados*/
                                var productoSkuInnova = _db.Producto.Where(p => p.Id == WorkOrderVM.DetalleOT[x].ProductoId).FirstOrDefault();
                                ;
                                var skuSplit = productoSkuInnova.SKU.Split("-");

                                var productoInnova = _db.Producto.
                                    Where(p => p.SKU == skuSplit[0] && p.EmpresaId == innovaId).FirstOrDefault();

                                double _saldo = GetSaldoProducto(innovaId, productoInnova);


                                var objDetalleMovimiento = new MovimientoDetail
                                {
                                    Cantidad = WorkOrderVM.DetalleOT[x].Cantidad,
                                    ProductoId = productoInnova.Id,
                                    MovimientoId = movIdInnova
                                };

                                /*Agrego stock a innova*/
                                objDetalleMovimiento.Saldo = _saldo + objDetalleMovimiento.Cantidad;
                                _db.MovimientoDetail.Add(objDetalleMovimiento);
                            }
                        }
                    }

                    #endregion              

                    await _db.SaveChangesAsync();
                    ViewBag.serverMessage = "Orden de Trabajo actualizada.";
                    return RedirectToAction(nameof(Index));
                }
                else
                    return NotFound();
            }

            return View(WorkOrderVM);
        }

        public JsonResult ActualizarEntregaOt(int id)
        {
            if (User.IsInRole(SD.Externo))
            {
                return Json("Usuario sin privilegios suficientes para ejecutar la operación.");
            }

            var otFromdb = _db.WorkOrder.Include(e => e.Empresa).Where(a => a.Id == id).FirstOrDefault();

            Producto producto = new Producto();


            if (otFromdb != null)
            {
                otFromdb.EstadoOT = EstadoOT.Entregado;
                var detalleFromdb = _db.WorkOrderDetail.Include(p => p.Producto).Where(d => d.WorkOrderId == otFromdb.Id).ToList();

                var movId = MovimientoSalida(otFromdb.EmpresaId);

                #region Logica salida stock EXTERNO

                if (detalleFromdb.Count > 0)
                {
                    //Ojo que no habrá ninguna pendiente porque la cantidad será 1 y solo 1
                    for (int i = 0; i < detalleFromdb.Count; i++)
                    {

                        var corteToProducto = _db.Producto.
                          Where(p => p.Id == detalleFromdb[i].ProductoId).FirstOrDefault();

                        //var skuSplit = corteToProductodb.SKU.Split("-");
                        /*Busqueda de productos para no generar duplicados*/
                        var productoOutExterno = _db.Producto.
                            Where(p => p.SKU == corteToProducto.SKU &&
                            p.EmpresaId == otFromdb.EmpresaId).FirstOrDefault();

                        /*SKU A PRODUCTO NORMAL*/

                        double _saldo = GetSaldoProducto(otFromdb.EmpresaId, productoOutExterno);

                        var objDetalleMovimiento = new MovimientoDetail
                        {
                            Cantidad = detalleFromdb[i].Cantidad,
                            ProductoId = productoOutExterno.Id,
                            MovimientoId = movId
                        };

                        objDetalleMovimiento.Saldo = _saldo - objDetalleMovimiento.Cantidad;
                        _db.MovimientoDetail.Add(objDetalleMovimiento);
                    }


                }
                #endregion

                /*agregar nuevo movimiento de stock para INNOVA. Ojo con sku*/

                var innovaId = _db.Empresa.Where(e => e.Rut == "76399752-9").Select(d => d.Id).FirstOrDefault();

                var movIdInnova = GetIdMovimientoEntrada(innovaId);

                if (detalleFromdb.Count > 0)
                {
                    for (int x = 0; x < detalleFromdb.Count; x++)
                    {
                        /*Busqueda de productos para no generar duplicados*/
                        var productoSkuInnova = _db.Producto.Where(p => p.Id == detalleFromdb[x].ProductoId).FirstOrDefault();
                        ;
                        var skuSplit = productoSkuInnova.SKU.Split("-");

                        var productoInnova = _db.Producto.
                            Where(p => p.SKU == skuSplit[0] && p.EmpresaId == innovaId).FirstOrDefault();

                        detalleFromdb[x].ProductoId = productoInnova.Id;
                        detalleFromdb[x].Producto = productoInnova; // quizas esto sea redundante, debo testear

                        double _saldo = GetSaldoProducto(innovaId, productoInnova);

                        var objDetalleMovimiento = new MovimientoDetail
                        {
                            Cantidad = detalleFromdb[x].Cantidad,
                            ProductoId = productoInnova.Id,
                            MovimientoId = movIdInnova
                        };

                        /*Agrego stock a innova*/
                        objDetalleMovimiento.Saldo = _saldo + objDetalleMovimiento.Cantidad;
                        _db.MovimientoDetail.Add(objDetalleMovimiento);
                    }
                }
                
                _db.SaveChanges();
                return Json($"Orden de trabajo {id} actualizada");
            }
            return Json($"No se encontró ninguna orden de trabajo con ID {id}.");


        }

        [HttpPost]
        public JsonResult RecepcionMultiple(List<int> idOrdenesTrabajo)
        {        
            
            string mensaje = "";
            

            if (idOrdenesTrabajo == null)
            {
                return Json("Sin ordenes de trabajo que procesar.");
            }

           
            foreach (var item in idOrdenesTrabajo)
            {
                var ordenesDeTrabajo = _db.WorkOrder.Where(c => c.Id == item).FirstOrDefault();

                if (ordenesDeTrabajo.EstadoOT != EstadoOT.Facturado)
                {
                    return Json($"La orden de trabajo con id {item} no se encuentra en estado Facturado");
                }

                var json = JsonConvert.SerializeObject(ActualizarEntregaOt(item).Value);
                mensaje += json + Environment.NewLine;
            }          


            return Json(mensaje);
        }


        public JsonResult ActualizarADespachoEntregado(int id)
        {

            var otFromdb = _db.WorkOrder.Include(e => e.Empresa).Where(a => a.Id == id).FirstOrDefault();
            otFromdb.EstadoOT = EstadoOT.Despachado;
            _db.SaveChanges();
            return Json("Estado de orden de trabajo actualizada a despachado.");

        }

        public JsonResult ReasignacionMain(int otFromdbID, int otReasignada, bool rollback = false)
        {
            try
            {
                //Traigo la orden del taller b y checkeo si es que el estado es facturado
                var _otTallerB = _db.WorkOrder.Where
                (t => t.Id == otReasignada).
                Include(e => e.Empresa).FirstOrDefault();

                if (_otTallerB.EstadoOT != EstadoOT.Facturado)
                {
                    return Json(new { mensaje = "El estado de la orden de trabajo asociada es distinta a Facturado. No se podrá hacer rollback." });
                }

                var otFromdb = _db.WorkOrder.Where(a => a.Id == otFromdbID).FirstOrDefault();
                var detalleOtFromdb = _db.WorkOrderDetail.Where(x => x.WorkOrderId == otFromdbID).ToList();

                if (detalleOtFromdb.Count > 1)
                    ViewBag.info = "No puede reasignar esta orden de trabajo ya que la relacion de " +
                        "productos debe ser 1:1";

                otFromdb.OrdenTrabajoReasignadaID = otReasignada;



                _otTallerB.OrdenTrabajoReasignadaID = otFromdbID;

                /*hacemos el switch entre talleres.*/
                var _tallerAEmpresaId = otFromdb.EmpresaId;
                var _tallerBEmpresaId = _otTallerB.EmpresaId;

                otFromdb.EmpresaId = _tallerBEmpresaId;

                if (rollback)
                {
                    _otTallerB.EstadoOT = EstadoOT.Facturado;
                    otFromdb.EstadoOT = EstadoOT.Facturado;
                    _otTallerB.OrdenTrabajoReasignadaID = null;
                    otFromdb.OrdenTrabajoReasignadaID = null;
                }
                else
                    _otTallerB.EstadoOT = EstadoOT.Reasignado;

                _otTallerB.EmpresaId = _tallerAEmpresaId;

                /*Debo reasignar los productos tambien */
                var detalleProductoB = _db.WorkOrderDetail.Where(x => x.WorkOrderId == _otTallerB.Id).
                    Include(e => e.WorkOrder.Empresa).Include(p => p.Producto).ToList();

                var detalleProductoA = _db.WorkOrderDetail.Where(y => y.WorkOrderId == otFromdb.Id).
                    Include(e => e.WorkOrder.Empresa).Include(p => p.Producto).ToList();

                for (int w = 0; w < detalleProductoB.Count; w++)
                {
                    int productoA = detalleProductoA[w].Producto.Id;
                    int productoB = detalleProductoB[w].Producto.Id;

                    detalleProductoA[w].ProductoId = productoB;
                    detalleProductoB[w].ProductoId = productoA;
                }
                _db.SaveChanges();
                return Json(new { mensaje = "Se actualizó a Reasignada la Orden de trabajo con ID " + _otTallerB.Id });
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return Json(new { mensaje = "Ha ocurrido un error, revise que las OT tengan relación 1:1." });
            }
        }
        private void EntradaStockExterno()
        {
            int movIdExterno = GetIdMovimientoEntrada(WorkOrderVM.WorkOrder.EmpresaId);

            if (WorkOrderVM.DetalleOT.Count > 0)
            {
                for (int x = 0; x < WorkOrderVM.DetalleOT.Count; x++)
                {
                    /*Busqueda de productos para no generar duplicados*/
                    var producto = _db.Producto.Where(p => p.Id == WorkOrderVM.DetalleOT[x].ProductoId).FirstOrDefault();
                    //var skuSplit = producto.SKU.Split("-");


                    var productoExterno = _db.Producto.
                        Where(p => p.SKU == producto.SKU && p.EmpresaId == WorkOrderVM.WorkOrder.EmpresaId).FirstOrDefault();


                    double _saldo = GetSaldoProducto(WorkOrderVM.WorkOrder.EmpresaId, productoExterno);

                    var movimientoEntradaExterno = new MovimientoDetail
                    {
                        Cantidad = WorkOrderVM.DetalleOT[x].Cantidad,
                        ProductoId = productoExterno.Id,
                        MovimientoId = movIdExterno
                    };
                    movimientoEntradaExterno.Saldo = _saldo + movimientoEntradaExterno.Cantidad;
                    _db.MovimientoDetail.Add(movimientoEntradaExterno);
                }

            }
        }
        private int MovimientoSalidaConSaldo(int i)
        {
            int _empresaID = WorkOrderVM.WorkOrder.EmpresaId;
            int movId = MovimientoSalida(_empresaID);

            var productoSKUBodega = _db.Producto.Where(p => p.SKU == WorkOrderVM.DetalleOT[i].Producto.SKU &&
           p.EmpresaId == _empresaID).FirstOrDefault();
            double _saldo = GetSaldoProducto(_empresaID, productoSKUBodega);

            _db.MovimientoDetail.Add(new MovimientoDetail
            {
                Cantidad = WorkOrderVM.DetalleOT[i].Cantidad,
                ProductoId = productoSKUBodega.Id,
                MovimientoId = movId,
                Saldo = _saldo - WorkOrderVM.DetalleOT[i].Cantidad
            });

            var detalleOtFromDB = _db.WorkOrderDetail.Where
                (d => d.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToList();
            detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
            detalleOtFromDB[i].Cantidad = WorkOrderVM.DetalleOT[i].Cantidad;

            return _empresaID;
        }

        private double GetSaldoProducto(int _empresaID, Producto producto)
        {
            var prodId = producto.Id;
            var detallSaldoDb = _db.MovimientoDetail.Include(p => p.Producto).Where(p => p.ProductoId == prodId &&
            p.Producto.EmpresaId == _empresaID).LastOrDefault();
            double _saldo = 0;

            if (detallSaldoDb != null)
                _saldo = detallSaldoDb.Saldo;
            return _saldo;
        }
        #endregion
        #region Cierre de Proceso
        [Authorize(Roles = SD.Control + "," + SD.Admin)]
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
                return NotFound();

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).Include(e => e.Cita).FirstOrDefaultAsync(m => m.Id == id);
            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == id).ToListAsync();
            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }
            return View(WorkOrderVM);
        }




        [Authorize(Roles = SD.Control + "," + SD.Admin + "," + SD.Externo)]
        public IActionResult Despachos()
        {
            return View();
        }
        #endregion

        public async Task<IActionResult> DepartureValidation(int? id)
        {
            if (id == null)
                return NotFound();

            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).Include(l => l.Cita).FirstOrDefaultAsync(m => m.Id == id);
            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == id).ToListAsync();
            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }
            return View(WorkOrderVM);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("DepartureValidation")]
        public async Task<IActionResult> DeparturePost()
        {
            string mensaje = string.Empty;
            bool flag = false;
            var detalleFromdb = _db.WorkOrderDetail.Where(w => w.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToList();

            for (int i = 0; i < WorkOrderVM.DetalleOT.Count; i++)
            {
                if (WorkOrderVM.DetalleOT[i].Lpn == WorkOrderVM.DetalleOT[i].LpnAux)
                {
                    detalleFromdb[i].IsReadyForDeparture = true;
                    flag = true;
                }
                else
                {
                    flag = false;
                    mensaje = "Lpn no coinciden.";
                }
            }

            if (flag)
            {
                TempData["srvMessage"] = "Se actualizó la Orden de trabajo con número " + WorkOrderVM.WorkOrder.Id;
                _db.SaveChanges();
                return RedirectToAction("Close", new { id = WorkOrderVM.WorkOrder.Id });
            }


            WorkOrderVM.WorkOrder = await _db.WorkOrder.Include(u => u.User).
                Include(l => l.Cita).FirstOrDefaultAsync(m => m.Id == WorkOrderVM.WorkOrder.Id);

            WorkOrderVM.DetalleOT = new List<WorkOrderDetail>();

            var objDetalle = await _db.WorkOrderDetail.Include(p => p.Producto).Where(p => p.WorkOrderId == WorkOrderVM.WorkOrder.Id).ToListAsync();

            foreach (var item in objDetalle)
            {
                WorkOrderVM.DetalleOT.Add(item);
            }

            ViewBag.mensaje = mensaje;
            return View(WorkOrderVM);

        }

        #region Reportes
        /// <summary>
        /// Función la cual retorna un archivo excel con un reporte de las entregas del externo a innova.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = SD.Control + "," + SD.Admin)]
        public IActionResult Report()
        {
            return View();
        }

        [HttpPost("Report"), ValidateAntiForgeryToken]
        public IActionResult ReportPost(DateTime desde, DateTime hasta)
        {
            if (desde == DateTime.MinValue)
            {
                ViewData["invalidModel"] = $"Fecha desde {desde} inválida.";
                return View(nameof(Report));
            }

            if (hasta == DateTime.MinValue)
            {
                hasta = DateTime.Now;
            }


            var lista = from objExcel in _db.WorkOrderDetail
                        where objExcel.WorkOrder.FechaCreacion >= desde && objExcel.WorkOrder.FechaCreacion <= hasta
                        select new
                        {
                            Externo = objExcel.WorkOrder.Empresa.Nombre,
                            Fecha_Creacion = objExcel.WorkOrder.FechaCreacion.ToShortDateString(),
                            objExcel.Producto.SKU,
                            objExcel.Producto.Nombre,
                            objExcel.Cantidad,
                            objExcel.Producto.PrecioCompra,
                            objExcel.WorkOrder.EstadoOT

                        };

            if (lista.Count() == 0)
            {
                ViewData["invalidModel"] = $"No se encontraron registros en la fecha especificada.";
                return View(nameof(Report));
            }

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("GetReporteHistorico");
                workSheet.Cells.LoadFromCollection(lista, true);
                package.Save();
            }
            stream.Position = 0;
            string excelName = $"Reporte_Innova_Historico-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);


        }

        #endregion

        #region Funciones
        public JsonResult GetDespachos()
        {
            try
            {
                /*Si el usuario es externo, verá solo sus ots.*/
                if (User.IsInRole(SD.Externo))
                {
                    var otFromDb = _db.WorkOrder.Include(e => e.Empresa).Where(p => p.EmpresaId == _empresaRepository.GetEmpresaIdByUser()
                    && p.EstadoOT == EstadoOT.Despachado);
                    var recordsTotal = otFromDb.Count();
                    var data = otFromDb.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                }
                else
                {
                    var otFromDb = _db.WorkOrder.Include(e => e.Empresa).Where(o => o.EstadoOT == EstadoOT.Despachado);
                    var recordsTotal = otFromDb.Count();
                    var data = otFromDb.ToList();
                    return Json(new { recordsFiltered = recordsTotal, data });
                }
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        public async Task<JsonResult> DesactivarDespacho(int id)
        {
            try
            {
                var ordenTrabajo = await _db.WorkOrder.Where(a => a.Id == id).FirstOrDefaultAsync();
                var detalleOt = await _db.WorkOrderDetail.Include(p => p.Producto).Where(d => d.WorkOrderId == ordenTrabajo.Id).ToListAsync();

                if (ordenTrabajo.IsActive)
                {
                    for (int i = 0; i < detalleOt.Count; i++)
                    {
                        int empresaId = _empresaRepository.GetEmpresaIdByRut("76399752-9");
                        int movId = MovimientoSalida(empresaId);

                        if (string.IsNullOrEmpty(detalleOt[i].MotivoDevolucion))
                        {

                            var productoEnDetalle = _db.Producto.Where(p => p.SKU == detalleOt[i].Producto.SKU &&
                              p.EmpresaId == empresaId).FirstOrDefault();

                            //sku y asignar producto id y producto

                            var skuSplit = productoEnDetalle.SKU.Split("-");


                            var productoSalida = _db.Producto.
                              Where(p => p.SKU == skuSplit[0] && p.EmpresaId == empresaId).FirstOrDefault();

                            double _saldo = GetSaldoProducto(empresaId, productoSalida);

                            _db.MovimientoDetail.Add(new MovimientoDetail
                            {
                                Cantidad = detalleOt[i].Cantidad,
                                ProductoId = productoSalida.Id,
                                MovimientoId = movId,
                                Saldo = _saldo - detalleOt[i].Cantidad
                            });

                            detalleOt[i].ProductoId = productoSalida.Id;
                            detalleOt[i].Producto = productoSalida;
                        }
                    }

                    ordenTrabajo.IsActive = false;
                    await _db.SaveChangesAsync();
                    return Json("Se ha finalizado la orden de trabajo n°" + id);
                }
                else
                {
                    return Json("La orden de trabajo n°" + id + " ya se encuentra finalizada.");
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }

        }

        public JsonResult GetResumenOrdenTrabajo()
        {
            List<WorkOrderDetail> listaOrdenesDeTrabajo = new List<WorkOrderDetail>();
            

            if (User.IsInRole(SD.Admin) || User.IsInRole(SD.Control))
            {
                listaOrdenesDeTrabajo = _db.WorkOrderDetail.Include(p => p.Producto).
                    Include(w => w.WorkOrder).ThenInclude(e => e.Empresa).
                    Where(x => x.WorkOrder.EstadoOT != EstadoOT.Despachado).ToList();

                var _TotalRecords = listaOrdenesDeTrabajo.Count();
                var data = listaOrdenesDeTrabajo.ToList();
                return Json(new { recordsFiltered = _TotalRecords, data });
            }
            else
            {
                

                listaOrdenesDeTrabajo = _db.WorkOrderDetail.Include(p => p.Producto).
                    Include(w => w.WorkOrder).ThenInclude(e => e.Empresa).
                    Where(x => x.WorkOrder.EmpresaId == _empresaRepository.GetEmpresaIdByUser() && 
                    x.WorkOrder.EstadoOT != EstadoOT.Despachado).ToList();
                
                foreach (var item in listaOrdenesDeTrabajo)
                {
                    item.Role = SD.Externo;
                }                

                var _TotalRecords = listaOrdenesDeTrabajo.Count();
                var data = listaOrdenesDeTrabajo.ToList();
                return Json(new { recordsFiltered = _TotalRecords, data });
            }

        }
        public JsonResult GetProductosFromWorkOrders(int? id)
        {
            if (id == null)
                return Json("No encontrado.");

            try
            {

                var detalleWorkOrder = _db.WorkOrderDetail.Include(p => p.Producto).Where(d => d.WorkOrderId == id);
                var recordsTotal = detalleWorkOrder.Count();
                var data = detalleWorkOrder.ToList();
                return Json(new { recordsFiltered = recordsTotal, data });

            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
        public JsonResult EliminarOT(int? id)
        {
            if (!User.IsInRole(SD.Admin))
                return Json("Usuario no tiene permisos para eliminar la orden de trabajo " + id);

            if (id == null)
                return Json("Error con la orden de trabajo: Id.");

            var ordenTrabajo = _db.WorkOrder.Where(o => o.Id == id).FirstOrDefault();

            if (ordenTrabajo == null)
                return Json("No se encontró la orden de trabajo");

            if (ordenTrabajo.EstadoOT != EstadoOT.Creado)
                return Json("No se puede eliminar una orden de trabajo con estado distinto a creado.");

            _db.Remove(ordenTrabajo);
            _db.SaveChanges();
            return Json("Orden de trabajo " + id + " eliminada.");

        }
        public JsonResult GetWorkOrder(int WorkOrderID)
        {
            var _objetoWorkOrder = _db.WorkOrderDetail.Include(p => p.Producto).Include(w => w.WorkOrder).ThenInclude(e => e.Empresa).Where(w => w.WorkOrderId == WorkOrderID);
            return Json(new { _objetoWorkOrder });
        }

        /// <summary>
        /// Función el cual retorna la id del objeto creado. En este caso, será un movimiento de entrada. 
        /// Recibe como parametro la id de la empresa.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetIdMovimientoEntrada(int id)
        {
            var _movimientoEntradaExterno = new Movimiento()
            {
                EmpresaId = id,
                FechaMovimiento = DateTime.Now,
                TipoOperacion = TipoOperacion.Entrada,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
            };
            _db.Movimiento.Add(_movimientoEntradaExterno);
            var movIdExterno = _movimientoEntradaExterno.Id;
            return movIdExterno;
        }
        /// <summary>
        ///  Función el cual retorna la id del objeto creado. En este caso, será un movimiento de salida. 
        /// Recibe como parametro la id de la empresa.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int MovimientoSalida(int id)
        {
            var _movimientoSalida = new Movimiento()
            {
                EmpresaId = id,
                FechaMovimiento = DateTime.Now,
                TipoOperacion = TipoOperacion.Salida,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
            };
            _db.Movimiento.Add(_movimientoSalida);
            var movId = _movimientoSalida.Id;
            return movId;
        }
        /// <summary>
        /// Función que envia email con cuenta de google certificada avisando sobre una orden de trabajo creada.
        /// </summary>
        /// <param name="empresaId"></param>
        /// <returns></returns>
        private bool EnvioMail(int empresaId)
        {
            //A uturo implementare el envio email, como no lo han pedido... pues lo dejamos standby
            var empresaFromDB = _db.Empresa.Where(e => e.Id == empresaId).FirstOrDefault();

            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;

            client.EnableSsl = true;

            client.Credentials = new NetworkCredential("certificaciones@netdte.cl", "netdte2018");

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("operaciones@innovamobel.cl", "Informática Innova Mobel");
            mail.To.Add("empresaFromDB.EmailEncargado");
            mail.To.Add("angel.pinilla@sisgenchile.cl");

            mail.Subject = "Notificación Orden de Trabajo";
            mail.BodyEncoding = Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            mail.IsBodyHtml = true;
            mail.Body = "Se ha generado una orden de trabajo con hora " + DateTime.Now.ToShortTimeString() + ". Debe " +
                "ingresar al sistema para ver los detalles." +

                "<br> Atte.<br> Operaciones de Informática de Innova Mobel";
            try
            {
                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public JsonResult GetInsumos()
        {
            var insumosFromDB = _db.Producto.Where(p => p.TipoProducto == TipoProducto.Corte && p.EmpresaId == _empresaRepository.GetIdInnovita());
            var _TotalRecords = insumosFromDB.Count();
            var data = insumosFromDB.ToList();
            return Json(new { recordsFiltered = _TotalRecords, data });
        }
        #endregion
    }
}
