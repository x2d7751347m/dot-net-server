using DotNetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetAPI.Data;

public class DataContextEf(IConfiguration config) : DbContext
{
    public virtual DbSet<User> Users {get; set;}
    public virtual DbSet<UserSalary> UserSalary {get; set;}
    public virtual DbSet<UserJobInfo> UserJobInfo {get; set;}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseMySql(config.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(11, 3, 0)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("Users")
            .HasKey(u => u.UserId);

        modelBuilder.Entity<UserSalary>()
            .ToTable("UserSalary")
            .HasKey(u => u.UserId);

        modelBuilder.Entity<UserJobInfo>()
            .ToTable("UserJobInfo")
            .HasKey(u => u.UserId);
    }
}