using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.Shared.Kernel.Events
{
    public class DeliveryJobAcceptedEvent
    {
        public Guid OrderId { get; set; }
        public Guid JobId { get; set; }
        public Guid PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public DateTime AcceptedAt { get; set; }
    }
}
