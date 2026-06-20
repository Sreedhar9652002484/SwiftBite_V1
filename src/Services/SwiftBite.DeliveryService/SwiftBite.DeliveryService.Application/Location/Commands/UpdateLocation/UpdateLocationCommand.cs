using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Application.Location.Commands.UpdateLocation
{
    // Command
    public record UpdateLocationCommand(
        string UserId,
        double Latitude,
        double Longitude) : IRequest;
}
