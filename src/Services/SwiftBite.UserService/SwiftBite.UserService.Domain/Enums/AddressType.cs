using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SwiftBite.UserService.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AddressType
    {
        Home = 1,
        Office = 2,
        Other = 3
    }
}
