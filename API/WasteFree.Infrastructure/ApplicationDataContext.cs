using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;

namespace WasteFree.Infrastructure;

public class ApplicationDataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}