using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SwiftBite.AuthServer.Data.Converters;

/// <summary>
/// Ensures DateTime values are always treated/persisted as UTC and read back
/// with Kind=Utc (SQL Server's datetime2 does not persist DateTimeKind, so
/// EF Core would otherwise return Kind=Unspecified, which serializes to JSON
/// without a trailing "Z" and gets misinterpreted as local time by clients).
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeConverter"/>.
/// </summary>
public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
