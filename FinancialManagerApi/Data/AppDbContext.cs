using FinancialManagerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<Transaction> Transactions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Password).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).IsRequired();
            entity.Property(t => t.Date).IsRequired();
            entity.Property(t => t.Type).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(255);
                
            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}