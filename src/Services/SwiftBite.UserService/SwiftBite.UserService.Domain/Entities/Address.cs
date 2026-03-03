using SwiftBite.Shared.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.UserService.Domain.Entities
{
    public class Address : BaseEntity
    {
        public string Label { get; set; }        // "Home", "Work"
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsDefault { get; set; }
        public Guid UserId { get; set; }
    }
}
