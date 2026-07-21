using CqrsExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace CqrsExample.Contexts;

public class CommandDbContext : DbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options) {}

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Outbox> Outbox { get; set; }
}