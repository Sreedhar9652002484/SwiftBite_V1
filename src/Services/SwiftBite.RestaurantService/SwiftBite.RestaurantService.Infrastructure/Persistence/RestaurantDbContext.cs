using Microsoft.EntityFrameworkCore;
using SwiftBite.RestaurantService.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(
        DbContextOptions<RestaurantDbContext> options)
        : base(options) { }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantHours> RestaurantHours => Set<RestaurantHours>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RestaurantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}