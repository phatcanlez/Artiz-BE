using Microsoft.EntityFrameworkCore;
using BO.Models;

namespace DAL;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Product3DFile> Product3DFiles { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // ProductImage configuration
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product3DFile configuration
        modelBuilder.Entity<Product3DFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Product3DFiles)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.ReviewerName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ReviewerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Comment).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5"));
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderInvoiceNumber).IsRequired().HasMaxLength(64);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // BlogPost configuration
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Slug).IsUnique();
        });
    }
}

