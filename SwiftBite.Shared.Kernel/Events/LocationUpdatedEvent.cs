using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.Shared.Kernel.Events
{
    // SwiftBite.Shared.Kernel/Events/LocationUpdatedEvent.cs
    public class LocationUpdatedEvent
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
