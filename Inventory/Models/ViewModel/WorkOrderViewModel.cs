using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Models.ViewModel
{
    public class WorkOrderViewModel
    {
        public WorkOrder WorkOrder { get; set; }
        public List<WorkOrderDetail> DetalleOT { get; set; }

        public IEnumerable<object> WorkOrdersList { get; set; }
    }
}
