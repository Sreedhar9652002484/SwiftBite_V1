using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.Shared.Kernel.Events
{
    public class DeliveryJobDeliveredEvent
    {
        public Guid OrderId { get; set; }
        public Guid JobId { get; set; }
        public Guid PartnerId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime DeliveredAt { get; set; }
    }

}
