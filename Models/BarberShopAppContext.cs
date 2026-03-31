using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Models;

public partial class BarberShopAppContext : DbContext
{
    public BarberShopAppContext()
    {
    }

    public BarberShopAppContext(DbContextOptions<BarberShopAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Barber> Barbers { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillProduct> BillProducts { get; set; }

    public virtual DbSet<BillService> BillServices { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA586240F02E2");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E46AE83950").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Accounts__RoleID__3A81B327");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA21FD2C0FD");

            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.BarberId).HasColumnName("BarberID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xử lý");

            entity.HasOne(d => d.Barber).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.BarberId)
                .HasConstraintName("FK__Appointme__Barbe__4AB81AF0");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Appointme__Custo__49C3F6B7");
        });

        modelBuilder.Entity<Barber>(entity =>
        {
            entity.HasKey(e => e.BarberId).HasName("PK__Barbers__BE22A88E6F939587");

            entity.HasIndex(e => e.Phone, "UQ__Barbers__5C7E359E4CC74D47").IsUnique();

            entity.Property(e => e.BarberId).HasColumnName("BarberID");
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CommissionRate).HasDefaultValue(0.0);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsBusy).HasDefaultValue(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__Bills__11F2FC4A12DA402F");

            entity.Property(e => e.BillId).HasColumnName("BillID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Chưa thanh toán");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Account).WithMany(p => p.Bills)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Bills__AccountID__4F7CD00D");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bills)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Bills__CustomerI__4E88ABD4");
        });

        modelBuilder.Entity<BillProduct>(entity =>
        {
            entity.HasKey(e => new { e.BillId, e.ProductId }).HasName("PK__Bill_Pro__DAB23024CD229C49");

            entity.ToTable("Bill_Products");

            entity.Property(e => e.BillId).HasColumnName("BillID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillProducts)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bill_Prod__BillI__59FA5E80");

            entity.HasOne(d => d.Product).WithMany(p => p.BillProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bill_Prod__Produ__5AEE82B9");
        });

        modelBuilder.Entity<BillService>(entity =>
        {
            entity.HasKey(e => new { e.BillId, e.ServiceId, e.BarberId }).HasName("PK__Bill_Ser__221D65ECE80E676F");

            entity.ToTable("Bill_Services");

            entity.Property(e => e.BillId).HasColumnName("BillID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.BarberId).HasColumnName("BarberID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Barber).WithMany(p => p.BillServices)
                .HasForeignKey(d => d.BarberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bill_Serv__Barbe__571DF1D5");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillServices)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bill_Serv__BillI__5535A963");

            entity.HasOne(d => d.Service).WithMany(p => p.BillServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bill_Serv__Servi__5629CD9C");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8BF6AF5CF");

            entity.HasIndex(e => e.Phone, "UQ__Customer__5C7E359EE116C8D7").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6EDDFD12D83");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Stock).HasDefaultValue(0);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A9D503EA8");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA66864864");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
