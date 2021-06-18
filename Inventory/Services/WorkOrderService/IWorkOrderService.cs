using Inventory.Models;
using System.Threading.Tasks;

namespace Inventory.Services.WorkOrderService
{
    public interface IWorkOrderService
    {
        Task<ServiceResponse<WorkOrder>> UpdateWorkOrder(WorkOrder updatedWorkOrder);
    }
}
