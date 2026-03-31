namespace SwiftBite.DeliveryService.Domain.Enums;

public enum VehicleType
{
    Bike = 1,
    Scooter = 2,
    Car = 3,
    Cycle = 4,
}

public enum PartnerStatus
{
    Active = 1,
    Inactive = 2,
    Banned = 3,
}

public enum JobStatus
{
    Assigned = 1,
    Accepted = 2,
    PickedUp = 3,
    Delivered = 4,
    Rejected = 5,
    Cancelled = 6,
}