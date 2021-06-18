using Inventory.Data;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Inventory.Services.WorkOrderService
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly ApplicationDbContext _db;
        public WorkOrderService(ApplicationDbContext db )
        {
            _db = db;
        }

        public async  Task<ServiceResponse<WorkOrder>> UpdateWorkOrder(WorkOrder updatedWorkOrder)
        {
            ServiceResponse<WorkOrder> serviceResponse = new ServiceResponse<WorkOrder>();

            try
            {
                var workOrder = await _db.WorkOrder.FirstOrDefaultAsync(w => w.Id == updatedWorkOrder.Id);

                workOrder.FechaTermino = updatedWorkOrder.FechaTermino;
                workOrder.Jornada = updatedWorkOrder.Jornada;
                workOrder.Comentario = updatedWorkOrder.Comentario;
                workOrder.NumeroFacturaExterno = updatedWorkOrder.NumeroFacturaExterno;
                
                _db.WorkOrder.Update(workOrder);
                
                await _db.SaveChangesAsync();

                serviceResponse.Data = workOrder;

            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}
