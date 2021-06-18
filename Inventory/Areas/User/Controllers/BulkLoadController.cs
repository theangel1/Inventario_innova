using Inventory.Data;
using Inventory.Models;
using Inventory.Services.EmpresaService;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Inventory.Areas.User.Controllers
{
    [Area("User")]
    public class BulkLoadController : Controller
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;


        public BulkLoadController(ApplicationDbContext db, HostingEnvironment hostingEnvironment, IEmpresaRepository empresaRepository)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            _empresaRepository = empresaRepository;

        }


        #region tramo_2
        [Authorize(Roles = SD.Control + "," + SD.Admin)]
        public IActionResult CargaMasivaOT()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaMasivaOT")]
        public IActionResult CargaMasivaPost()
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (files.Count != 0)
            {
                var uploads = Path.Combine(webRootPath, SD.folder);

                var _arcFile = "OrdenTrabajo" + DateTime.Now.ToString("ddMMHHyyyymmssFFF") + ".xlsx";

                using (var filestream = new FileStream(Path.Combine(
                    uploads, _arcFile), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                FileInfo existingFile = new FileInfo(webRootPath + @"\" + SD.folder + @"\" + _arcFile);

                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int contadorCol = worksheet.Dimension.End.Column;
                    int contadorRow = worksheet.Dimension.End.Row;
                    var _listaDetallesOTExcel = new List<WorkOrderDetail>();

                    /*Validación de columnas. Deben venir 2 
                     {ordenTrabajo | rut externo}
                     */
                    if (contadorCol != 2)
                    {
                        ViewBag.serverError = "¡Archivo mal formado, las columnas no coinciden!";
                        return View();
                    }

                    for (int filas = 2; filas <= contadorRow; filas++)
                    {
                        if (worksheet.Cells[filas, 1].Value == null)
                        {
                            ViewBag.serverError = "Orden de trabajo no puede venir vacio en la fila " + filas;
                            return View();
                        }

                        if (worksheet.Cells[filas, 2].Value == null)
                        { 
                            ViewBag.serverError = "Rut no puede venir vacio en la fila " + filas;
                            return View();
                        }

                        var idOtExcel = worksheet.Cells[filas, 1].Value.ToString().Trim();
                        string rutExternoExcel = worksheet.Cells[filas, 2].Value.ToString().ToUpper().Trim();


                        if (!int.TryParse(idOtExcel, out int idOrdenTrabajo))
                        {
                            ViewBag.serverError = "N° de orden de trabajo no es un número en la fila " + filas;
                            return View();
                        }

                        var ordenTrabajoFromdb = _db.WorkOrder.Where(o => o.Id == idOrdenTrabajo).FirstOrDefault();

                        if (ordenTrabajoFromdb == null)
                        {
                            ViewBag.serverError = "No se encontró la orden de trabajo con número " + idOtExcel;
                            return View();
                        }

                        if (_empresaRepository.GetEmpresaIdByRut(rutExternoExcel) == 0)
                        {
                            ViewBag.serverError = "No se encontró la empresa con Rut " + rutExternoExcel;
                            return View();
                        }

                        ordenTrabajoFromdb.EmpresaId = _empresaRepository.GetEmpresaIdByRut(rutExternoExcel);

                        if (ordenTrabajoFromdb.EmpresaId == 1) /*Si es innova, pasa directoa  entregado por regla de negocio*/
                            ordenTrabajoFromdb.EstadoOT = EstadoOT.Entregado;
                        else
                            ordenTrabajoFromdb.EstadoOT = EstadoOT.Asignado;


                        //validacion de estado
                        if ( ordenTrabajoFromdb.EstadoOT != EstadoOT.Asignado && ordenTrabajoFromdb.EstadoOT != EstadoOT.Entregado)
                        {
                            ViewBag.serverError = "El estado de la orden de trabajo n° " + ordenTrabajoFromdb.Id + " es distinto a Asignado/Entregado.";
                            return View();
                        }

                        _db.Update(ordenTrabajoFromdb);


                    }

                }

                _db.SaveChanges();
                existingFile.Delete();
                return RedirectToAction("Index", "WorkOrders", new { area = "User" });
            }
            return RedirectToAction("CargaMasivaOT", new { mensaje = "¡Debes seleccionar el archivo excel!" });
        }

        #endregion

        #region Carga LPN
        public IActionResult CargaMasivaLPN()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaMasivaLPN")]
        public IActionResult CargaMasivaLPNPost()
        {
            /*La idea de esta funcion, es actualizar las ordenes de trabajo ya agregadas con los 
             nuevos campos los cuales son: NumeroFacturaRetail, LPN,NumeroCita,PatenteCamion*/

            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;


            if (files.Count != 0)
            {
                var uploads = Path.Combine(webRootPath, SD.folderLpn);
                var _arcFile = "OrdenTrabajoLPN" + DateTime.Now.ToString("ddMMHHyyyymmssFFF") + ".xlsx";

                using (var filestream = new FileStream(Path.Combine(
                    uploads, _arcFile), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                FileInfo existingFile = new FileInfo(webRootPath + @"\" + SD.folderLpn + @"\" + _arcFile);

                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int contadorCol = worksheet.Dimension.End.Column;
                    int contadorRow = worksheet.Dimension.End.Row;
                    string ordenCompra = string.Empty;
                    var _listaDetallesOTExcel = new List<WorkOrderDetail>();
                    int contadorOrdenCompra = 0;

                    if (contadorCol != 12)
                    {
                        ViewBag.serverError = "¡Archivo LPN mal formado, las columnas no coinciden! Reintenta descargando el archivo excel base y agregando " +
                            "nuevamente sus LPN.";
                        return View();
                    }

                    for (int filas = 2; filas <= contadorRow; filas++)
                    {
                        if (worksheet.Cells[filas, 1].Value == null)
                            break;
                        try
                        {
                            var _ordenCompraFromExcel = worksheet.Cells[filas, 1].Value.ToString().Trim();
                            string lpn = "";

                            if (worksheet.Cells[filas, 9].Value != null)
                            {
                                lpn = worksheet.Cells[filas, 9].Value.ToString().Trim();
                            }


                            var skuProducto = worksheet.Cells[filas, 5].Value.ToString().Trim();
                            contadorOrdenCompra++;

                            if (worksheet.Cells[filas, 8].Value == null)
                            {
                                ViewBag.serverError = "No se encontró número factura retail en la fila " + filas;
                                return View();
                            }                         
                            else if (worksheet.Cells[filas, 10].Value == null)
                            {
                                ViewBag.serverError = "No se encontró Número de Cita en la fila " + filas;
                                return View();
                            }
                            else if (worksheet.Cells[filas, 11].Value == null)
                            {
                                ViewBag.serverError = "No se encontró patente en la fila " + filas;
                                return View();
                            }
                            else if (worksheet.Cells[filas, 12].Value == null)
                            {
                                ViewBag.serverError = "No se encontró hora de salida en la fila " + filas;
                                return View();
                            }

                            var ordenTrabajo = _db.WorkOrder.Where(o => o.OrdenCompra == _ordenCompraFromExcel).FirstOrDefault();

                            
                            //validacion existencia de lpn previo
                            var detalleFromdb = _db.WorkOrderDetail.Where(w => w.Lpn !="" && w.Lpn == lpn).FirstOrDefault();                            
                            if (detalleFromdb !=null)
                            {
                                ViewBag.serverError = "El LPN " + lpn + " ya se encuentra asociado a la orden de trabajo " + detalleFromdb.WorkOrderId;
                                return View();
                            }

                            if (ordenTrabajo is null)
                            {
                                ViewBag.serverError = "No se encontró Orden de trabajo asociada para la " +
                                    "orden de compra " + _ordenCompraFromExcel;
                                return View();
                            }


                            var detalleOrdenesTrabajo = _db.WorkOrderDetail.Include(a => a.WorkOrder).Where(a => a.WorkOrder.OrdenCompra == ordenTrabajo.OrdenCompra).ToList();

                            if (detalleOrdenesTrabajo.Count > 1)
                            {

                                if (contadorOrdenCompra == detalleOrdenesTrabajo.Count())
                                {
                                    detalleOrdenesTrabajo.LastOrDefault().Lpn = lpn;
                                    contadorOrdenCompra = 0;
                                }
                                else
                                {
                                    detalleOrdenesTrabajo[contadorOrdenCompra - 1].Lpn = lpn;
                                }

                            }
                            else
                            {
                                detalleOrdenesTrabajo.FirstOrDefault().Lpn = lpn;
                                contadorOrdenCompra = 0;
                            }



                            var cita = new Cita()
                            {
                                NumeroCita = worksheet.Cells[filas, 10].Value.ToString().Trim(),
                                Patente = worksheet.Cells[filas, 11].Value.ToString().Trim().ToUpper(),
                                HoraSalidaCamion = worksheet.Cells[filas, 12].Value.ToString().Trim().ToUpper(),
                            };


                            //si no existe una cita, la creo
                            if (ordenTrabajo.CitaID is null)
                            {
                                _db.Cita.Add(cita);
                                ordenTrabajo.CitaID = cita.Id;
                            }
                            else
                            {
                                var citaDB = _db.Cita.Where(a => a.Id == ordenTrabajo.CitaID).FirstOrDefault();
                                citaDB = cita;
                            }


                            ordenTrabajo.NumeroFacturaRetail = int.Parse(worksheet.Cells[filas, 8].Value.ToString().Trim());
                            ordenCompra = _ordenCompraFromExcel;
                            _db.SaveChanges();


                        }
                        catch (Exception ex)
                        {

                            ViewBag.serverError = "Hay un error con el formato del archivo excel. ";
                            return View();
                        }
                    }


                }
                existingFile.Delete();
                //Si llegamos hasta acá, es porque sobrevivimos al 19/10/2019- Implementar envio email acá


                //return RedirectToAction("Index", "WorkOrders", new { area = "User" , mensaje = "¡Todos los LPN fueron cargados exitosamente!"});
                return RedirectToAction("CargaMasivaLPN", new { mensaje = "¡Todos los LPN fueron cargados exitosamente!" });

            }
            return RedirectToAction("CargaMasivaLPN", new { mensaje = "¡Debes seleccionar el archivo excel!" });


        }

        #endregion

        #region Carga Stock

        public IActionResult CargaMasivaStock()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaMasivaStock")]
        public IActionResult CargaMasivaPostStock()
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (files.Count == 0)
            {
                ViewBag.Message = "¡¡¡Debes seleccionar un archivo excel!!!";
                return View();
            }

            var uploads = Path.Combine(webRootPath, SD.folder);
            var _arcFile = "Movimiento" + DateTime.Now.ToString("ddMMHHyyyymmssFFF") + ".xlsx";

            using (var filestream = new FileStream(Path.Combine(uploads, _arcFile), FileMode.Create))
            {
                files[0].CopyTo(filestream);
            }

            FileInfo existingFile = new FileInfo(webRootPath + @"\" + SD.folder + @"\" + _arcFile);

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int contadorCol = worksheet.Dimension.End.Column;
                int contadorRow = worksheet.Dimension.End.Row;
                var _listaMovimientos = new List<MovimientoDetail>();

                if (contadorCol != 3)
                {
                    ViewBag.Message = "¡Archivo mal formado, las columnas no coinciden!";
                    return View();
                }

                try
                {

                    for (int filas = 2; filas <= contadorRow; filas++)
                    {
                        var prodFromExcel = worksheet.Cells[filas, 2].Value.ToString().Trim();

                        if (prodFromExcel.Length < 1)
                        {
                            ViewBag.Message = "SKU del archivo excel mal formado. Ver Fila N° " + filas;
                            return View();
                        }

                        var productoFromdb = _db.Producto.Where(p => p.EmpresaId == _empresaRepository.GetEmpresaIdByRut(worksheet.Cells[filas, 1].Value.ToString().Trim())
                               && p.SKU == prodFromExcel).Select(e => e.Id).FirstOrDefault();

                        var saldoFromdb = _db.MovimientoDetail.Where(p => p.ProductoId == productoFromdb &&
                        p.Movimiento.EmpresaId == _empresaRepository.GetEmpresaIdByRut(worksheet.Cells[filas, 1].Value.ToString().Trim())).LastOrDefault();

                        double _saldo = 0;

                        if (saldoFromdb != null)
                            _saldo = saldoFromdb.Saldo;

                        var detalleMov = new MovimientoDetail()
                        {
                            Cantidad = int.Parse(worksheet.Cells[filas, 3].Value.ToString().Trim()),
                            ProductoId = productoFromdb,
                        };

                        detalleMov.Saldo = _saldo + detalleMov.Cantidad;
                        _listaMovimientos.Add(detalleMov);
                    }

                    var _movimiento = new Movimiento()
                    {
                        UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                        TipoOperacion = TipoOperacion.Entrada,
                        FechaMovimiento = DateTime.Now,
                        EmpresaId = _empresaRepository.GetEmpresaIdByRut(worksheet.Cells[2, 1].Value.ToString().Trim())
                    };
                    _db.Movimiento.Add(_movimiento);
                    int movId = _movimiento.Id;

                    for (int i = 0; i < _listaMovimientos.Count; i++)
                    {
                        _listaMovimientos[i].MovimientoId = movId;
                        _db.MovimientoDetail.Add(_listaMovimientos[i]);
                    }
                }
                catch (Exception ex)
                {

                    ViewBag.Message = "Ups, ha ocurrido un error inesperado. Debes contactarte con el programador si el problema persiste. ";
                    return View();
                }
            }

            _db.SaveChanges();
            ViewBag.MessageSuccess = "Stock cargado con éxito. ";

            return View();
        }

        #endregion

        #region tramo_1
        public IActionResult CargaA()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaA")]
        public IActionResult CargaAPost()
        {

            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (files.Count != 0)
            {
                var uploads = Path.Combine(webRootPath, SD.folder);

                var _arcFile = "tramo1" + DateTime.Now.ToString("ddMMHHyyyymmssFFF") + ".xlsx";

                using (var filestream = new FileStream(Path.Combine(
                    uploads, _arcFile), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                FileInfo existingFile = new FileInfo(webRootPath + @"\" + SD.folder + @"\" + _arcFile);

                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int contadorCol = worksheet.Dimension.End.Column;
                    int contadorRow = worksheet.Dimension.End.Row;
                    var _listaDetallesOTExcel = new List<WorkOrderDetail>();

                    /*Validación de columnas. Deben venir 7 
                     {ordenTrabajo	|FechaTermino	|Jornada	|SKU|	Cantidad | NombreRetail}
                     */
                    if (contadorCol != 6)
                    {
                        ViewBag.serverError = "¡Archivo mal formado, las columnas no coinciden! Revise si el archivo excel corresponde a este menú.";
                        return View();
                    }

                    for (int filas = 2; filas <= contadorRow; filas++)
                    {
                        if (worksheet.Cells[filas, 1].Value == null)
                            break;
                        try
                        {
                            /*Debo buscar el producto en bd con parametro sku el cual viene en el excel*/
                            var producto = worksheet.Cells[filas, 4].Value.ToString().Trim();

                            string cantidadProductoExcel = worksheet.Cells[filas, 5].Value.ToString().Trim();

                            if (int.TryParse(cantidadProductoExcel, out int valor))
                            {
                                if (valor > 1)
                                {
                                    ViewBag.serverError = "Cantidad del producto no puede ser  mayor a 1. Ver Fila N° " + filas;
                                    return View();
                                }
                            }
                            else
                            {
                                ViewBag.serverError = "Cantidad del producto no es un número. Ver Fila N° " + filas;
                                return View();
                            }


                            if (producto.Length < 4)
                            {
                                ViewBag.serverError = "SKU del archivo excel mal formado. Ver Fila N° " + filas;
                                return View();
                            }


                            IQueryable<Producto> productoFromdb;

                            //no es un corte

                            //se usa plantilla de producto de innovita
                            productoFromdb = _db.Producto.Where(p => p.EmpresaId == GetIdInnovita() && p.SKU == producto);


                            if (productoFromdb.Count() < 1)
                            {
                                ViewBag.serverError = "No se encontró en la base de datos el producto con SKU " + producto;
                                return View();
                            }

                            var idProducto = productoFromdb.Select(e => e.Id).First();

                            DateTime fechaTermino = DateTime.ParseExact(worksheet.Cells[filas, 2].Text.ToString(), "dd/MM/yyyy", CultureInfo.CreateSpecificCulture("es-ES"));

                            

                            /*  if ( !DateTime.TryParse(fechaTermino, out DateTime fechaTerminoParseada ))
                              {
                                  ViewBag.serverError = "Fecha de término inválida. Ver Fila N° " + filas;
                                  return View();
                              }                           */


                            var detallesOT = new WorkOrderDetail()
                            {
                                ProductoId = idProducto,
                                OrdenCompraAux = worksheet.Cells[filas, 1].Value.ToString().ToUpper(),
                                FechaTerminoAux =fechaTermino,
                                JornadaAux = worksheet.Cells[filas, 3].Value.ToString().ToUpper(),
                                Cantidad = int.Parse(worksheet.Cells[filas, 5].Value.ToString().Trim()),
                                NombreRetailAux = worksheet.Cells[filas, 6].Value.ToString().Trim().ToUpper(),
                                EmpresaIDAux = GetIdInnovita().ToString(),
                            };

                     

                            _listaDetallesOTExcel.Add(detallesOT);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.serverError = "DevOps: Hay un error con el formato del archivo excel. Error: " + ex.Message.ToString();
                            return View();
                        }

                    }
                    var woList = new List<WorkOrderDetail>();
                    bool flag = false;

                    for (int i = 0; i < _listaDetallesOTExcel.Count; i++)
                    {
                        if (i == (_listaDetallesOTExcel.Count - 1))
                        {
                            if (_listaDetallesOTExcel.Count == 1)
                            {
                                woList.Add(_listaDetallesOTExcel[i]);
                                flag = true;
                            }
                            else if (_listaDetallesOTExcel[i].OrdenCompraAux.SequenceEqual(_listaDetallesOTExcel[i - 1].OrdenCompraAux)
                                || !_listaDetallesOTExcel[i].OrdenCompraAux.SequenceEqual(_listaDetallesOTExcel[i - 1].OrdenCompraAux))
                            {
                                woList.Add(_listaDetallesOTExcel[i]);
                                flag = true;
                            }

                        }
                        else if (i == 0)
                        {
                            woList.Add(_listaDetallesOTExcel[i]);
                            flag = true;
                        }
                        else if (_listaDetallesOTExcel[i].OrdenCompraAux.SequenceEqual(_listaDetallesOTExcel[i + 1].OrdenCompraAux))
                        {
                            woList.Add(_listaDetallesOTExcel[i]);
                            flag = true;
                        }

                        else if (_listaDetallesOTExcel[i].OrdenCompraAux.SequenceEqual(_listaDetallesOTExcel[i - 1].OrdenCompraAux))
                        {
                            woList.Add(_listaDetallesOTExcel[i]);
                            flag = true;
                        }
                        else if (_listaDetallesOTExcel[i].OrdenCompraAux != _listaDetallesOTExcel[i + 1].OrdenCompraAux)
                        {
                            woList.Add(_listaDetallesOTExcel[i]);
                            flag = true;
                        }



                        if (flag)
                        {
                            var _workOrder = new WorkOrder()
                            {
                                EstadoOT = (_empresaRepository.GetEmpresaIdByRut(woList[0].EmpresaIDAux) == 1) ? EstadoOT.Entregado : EstadoOT.Creado,
                                IsActive = true,
                                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                                Jornada = woList[0].JornadaAux.ToString().ToUpper() == "AM" ? Jornada.AM : Jornada.PM,
                                EmpresaId = GetIdInnovita(),
                                FechaCreacion = DateTime.Now,
                                FechaTermino = DateTime.TryParse(woList[0].FechaTerminoAux.ToString(), out DateTime dateEnd) ? dateEnd : DateTime.Now.AddDays(5),
                                OrdenCompra = woList[0].OrdenCompraAux,
                                NombreRetail = woList[0].NombreRetailAux
                            };

                            _db.WorkOrder.Add(_workOrder);

                            for (int w = 0; w < woList.Count; w++)
                            {
                                woList[w].WorkOrderId = _workOrder.Id;
                                _db.WorkOrderDetail.Add(woList[w]);
                            }
                            woList.Clear();
                            flag = false;
                        }
                    }
                }
                
                existingFile.Delete();
                _db.SaveChanges();
                return RedirectToAction("Index", "WorkOrders", new { area = "User" });
            }
            return RedirectToAction("CargaA", new { mensaje = "¡Debes seleccionar el archivo excel!" });

        }

        #endregion

        #region Carga Cortes

        public IActionResult CargaMasivaCortes()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("CargaMasivaCortes")]
        public IActionResult CargaMasivaCortesPost()
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _hostingEnvironment.WebRootPath;

            if (files.Count == 0)
            {
                ViewBag.Message = "¡Debes seleccionar un archivo excel!";
                return View();
            }

            var uploads = Path.Combine(webRootPath, SD.folder);
            var _arcFile = "cortesExternos" + DateTime.Now.ToString("ddMMHHyyyymmssFFF") + ".xlsx";

            using (var filestream = new FileStream(Path.Combine(uploads, _arcFile), FileMode.Create))
            {
                files[0].CopyTo(filestream);
            }

            FileInfo existingFile = new FileInfo(webRootPath + @"\" + SD.folder + @"\" + _arcFile);

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int contadorCol = worksheet.Dimension.End.Column;
                int contadorRow = worksheet.Dimension.End.Row;

                if (contadorCol != 2)
                {
                    ViewBag.serverError = "¡Archivo excel mal formado o no corresponde!, ¡Las columnas no coinciden!";
                    return View();
                }

             

                    for (int filas = 2; filas <= contadorRow; filas++)
                    {
                        bool res = true;

                        

                        res = int.TryParse(worksheet.Cells[filas, 2].Value.ToString().Trim(), out int numeroFactura);

                        if (!res)
                        {
                            ViewBag.serverError = "Error con el número de factura en la fila " + filas;
                            return View();
                        }

                        string idOTExcel = worksheet.Cells[filas, 1].Value.ToString().Trim();

                        if (!int.TryParse(idOTExcel, out int idOT))
                        {
                            ViewBag.serverError = "Error con la id de orden de trabajo en la fila " + filas;
                            return View();
                        }

                        var workOrderFromdb = _db.WorkOrder.Where(w => w.Id == idOT).FirstOrDefault();

                        if (workOrderFromdb == null)
                        {
                            ViewBag.serverError = "No se encontró orden de compra n° " + idOT + " en la Fila N° " + filas;
                            return View();
                        }

                        //regla de solicitado

                        if (workOrderFromdb.EstadoOT != EstadoOT.Solicitado)
                        {
                            ViewBag.Message = "La orden de trabajo n° " + workOrderFromdb.Id + " no se encuentra en estado solicitado. Orden de trabajo n° " +
                                workOrderFromdb.Id + " en la fila n° " + filas;
                            return View();
                        }

                        //Asignacion de variables nuevas. Hay un template en el POST de edit de work orders

                        workOrderFromdb.NumeroFacturaExterno = numeroFactura;
                        workOrderFromdb.EstadoOT = EstadoOT.Facturado;

                        //Logica de entrada-salida

                        var detalleOTFromdb = new List<WorkOrderDetail>();
                        var objectoDetalle = _db.WorkOrderDetail.Where(d => d.WorkOrderId == workOrderFromdb.Id).Include(p => p.Producto).ToList();
                        int _empresaID = 0;


                        for (int i = 0; i < objectoDetalle.Count; i++)
                        {
                            if (objectoDetalle[i].Cantidad == objectoDetalle[i].CantidadNueva)
                            {
                                _empresaID = GetIdInnovita();
                                int movId = MovimientoSalida(_empresaID);
                                /*Busqueda de productos para no generar duplicados*/
                                var productoSKUBodega = _db.Producto.Where(p => p.SKU == objectoDetalle[i].Producto.SKU &&
                               p.EmpresaId == _empresaID).FirstOrDefault();

                                double _saldo = GetSaldoProducto(_empresaID, productoSKUBodega);

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = objectoDetalle[i].Cantidad,
                                    ProductoId = productoSKUBodega.Id,
                                    MovimientoId = movId,
                                    Saldo = _saldo - objectoDetalle[i].Cantidad
                                });

                                var detalleOtFromDB = _db.WorkOrderDetail.Where
                                    (d => d.WorkOrderId == workOrderFromdb.Id).ToList();

                                detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
                                detalleOtFromDB[i].Cantidad = objectoDetalle[i].Cantidad;
                                _empresaID = workOrderFromdb.EmpresaId;
                            }
                            else if (objectoDetalle[i].CantidadNueva == 0)
                            {
                                _empresaID = MovimientoSalidaConSaldo(i, _empresaID, objectoDetalle);
                            }
                            else if (objectoDetalle[i].CantidadNueva < objectoDetalle[i].Cantidad)
                            {
                                _empresaID = GetIdInnovita();

                                int movId = MovimientoSalida(_empresaID);

                                var productoSKUBodega = _db.Producto.Where(p => p.SKU == objectoDetalle[i].Producto.SKU &&
                               p.EmpresaId == _empresaID).FirstOrDefault();


                                var detallSaldoDb = _db.MovimientoDetail.Include(x => x.Movimiento).Where
                                    (p => p.ProductoId == objectoDetalle[i].ProductoId &&
                                    p.Movimiento.EmpresaId == _empresaID).LastOrDefault();

                                if (detallSaldoDb == null)
                                {
                                    ViewBag.serverError = "Error al recuperar el saldo.";
                                    return View();
                                }

                                double _saldo = 0;

                                if (detallSaldoDb != null)
                                    _saldo = detallSaldoDb.Saldo;

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = objectoDetalle[i].CantidadNueva,
                                    ProductoId = productoSKUBodega.Id,
                                    MovimientoId = movId,
                                    Saldo = _saldo - objectoDetalle[i].CantidadNueva
                                });

                                /*cantidad restante sale de bodega de externo*/

                                _empresaID = workOrderFromdb.EmpresaId;

                                int idMovExterno = MovimientoSalida(_empresaID);

                                var productoSKUBodegaExterno = _db.Producto.Where(p => p.SKU == objectoDetalle[i].Producto.SKU &&
                               p.EmpresaId == _empresaID).FirstOrDefault();


                                _saldo = GetSaldoProducto(_empresaID, productoSKUBodegaExterno);

                                _db.MovimientoDetail.Add(new MovimientoDetail
                                {
                                    Cantidad = objectoDetalle[i].Cantidad - objectoDetalle[i].CantidadNueva,
                                    ProductoId = productoSKUBodegaExterno.Id,
                                    MovimientoId = idMovExterno,
                                    Saldo = _saldo - (objectoDetalle[i].Cantidad - objectoDetalle[i].CantidadNueva)
                                });

                                var detalleOtFromDB = _db.WorkOrderDetail.Where
                                    (d => d.WorkOrderId == workOrderFromdb.Id).ToList();
                                detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
                                detalleOtFromDB[i].Cantidad = objectoDetalle[i].Cantidad;
                            }
                        }


                        EntradaStockExterno(_empresaID, objectoDetalle);

                    }

                    _db.SaveChanges();
                    ViewBag.MessageSuccess = "Orden de trabajo con productos solicitados actualizada.";
                    return View();            
            }
            existingFile.Delete();
        }

        #endregion

        private void EntradaStockExterno(int idEmpresa, List<WorkOrderDetail> detalle)
        {
            int movIdExterno = MovimientoEntrada(idEmpresa);

            if (detalle.Count > 0)
            {
                for (int x = 0; x < detalle.Count; x++)
                {
                    /*Busqueda de productos para no generar duplicados*/
                    var productoSkuExterno = _db.Producto.Where(p => p.SKU == detalle[x].Producto.SKU && p.EmpresaId == idEmpresa).FirstOrDefault();
                    //var skuSplit = productoSkuExterno.SKU.Split("-");


                    //var productoExterno = _db.Producto.
                      //  Where(p => p.SKU == skuSplit[0] && p.EmpresaId == idEmpresa).FirstOrDefault();


                    double _saldo = GetSaldoProducto(idEmpresa, productoSkuExterno);

                    var movimientoEntradaExterno = new MovimientoDetail
                    {
                        Cantidad = detalle[x].Cantidad,
                        ProductoId = productoSkuExterno.Id,
                        MovimientoId = movIdExterno
                    };
                    movimientoEntradaExterno.Saldo = _saldo + movimientoEntradaExterno.Cantidad;
                    _db.MovimientoDetail.Add(movimientoEntradaExterno);
                }

            }
        }

        private int MovimientoEntrada(int id)
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

        public int GetIdInnovita()
        {
            return _db.Empresa.Where(e => e.Rut == "76929437-6").Select(e => e.Id).First();
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

        private int MovimientoSalida(int idempresa)
        {
            var _movimientoSalida = new Movimiento()
            {
                EmpresaId = idempresa,
                FechaMovimiento = DateTime.Now,
                TipoOperacion = TipoOperacion.Salida,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
            };
            _db.Movimiento.Add(_movimientoSalida);
            var movId = _movimientoSalida.Id;
            return movId;
        }

        private int MovimientoSalidaConSaldo(int i, int idEmpresa, List<WorkOrderDetail> detalle)
        {
            int _empresaID = idEmpresa;
            int movId = MovimientoSalida(_empresaID);

            var productoSKUBodega = _db.Producto.Where(p => p.SKU == detalle[i].Producto.SKU &&
           p.EmpresaId == _empresaID).FirstOrDefault();
            double _saldo = GetSaldoProducto(_empresaID, productoSKUBodega);

            _db.MovimientoDetail.Add(new MovimientoDetail
            {
                Cantidad = detalle[i].Cantidad,
                ProductoId = productoSKUBodega.Id,
                MovimientoId = movId,
                Saldo = _saldo - detalle[i].Cantidad
            });

            var detalleOtFromDB = _db.WorkOrderDetail.Where
                (d => d.WorkOrderId == idEmpresa).ToList();
            detalleOtFromDB[i].ProductoId = productoSKUBodega.Id;
            detalleOtFromDB[i].Cantidad = detalle[i].Cantidad;

            return _empresaID;
        }
    }
}
