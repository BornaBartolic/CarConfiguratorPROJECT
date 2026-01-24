using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CarConfigDATA.Models;

public partial class AutoConfigDbContext : DbContext
{
    public AutoConfigDbContext()
    {
    }

    public AutoConfigDbContext(DbContextOptions<AutoConfigDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CarComponent> CarComponents { get; set; }

    public virtual DbSet<ComponentType> ComponentTypes { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<CarComponentCompatibility> CarComponentCompatibilities { get; set; }

    public virtual DbSet<CarConfiguration> CarConfigurations { get; set; }
    public virtual DbSet<CarConfigurationComponent> CarConfigurationComponents { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CarCompo__3214EC074843BD1D");

            entity.ToTable("CarComponent");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.ComponentType).WithMany(p => p.CarComponents)
                .HasForeignKey(d => d.ComponentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarComponent_ComponentType");
        });

        modelBuilder.Entity<CarComponentCompatibility>(entity =>
        {
            entity.HasKey(c => c.Id); // koristi novi Id

            entity.ToTable("CarComponentCompatibility");

            entity.HasOne(c => c.CarComponent1)
                  .WithMany()
                  .HasForeignKey(c => c.CarComponentId1)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_CarComponentCompatibility_1");

            entity.HasOne(c => c.CarComponent2)
                  .WithMany()
                  .HasForeignKey(c => c.CarComponentId2)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_CarComponentCompatibility_2");
        });


        modelBuilder.Entity<ComponentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Componen__3214EC07DEA58601");

            entity.ToTable("ComponentType");

            entity.HasIndex(e => e.Name, "UQ__Componen__737584F62537C895").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Log__3214EC07056C8A72");

            entity.ToTable("Log");

            entity.Property(e => e.Level).HasMaxLength(20);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC07EEEA49B0");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "UQ__Role__737584F6581468EA").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC077B65E8B8");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E4857DB1CA").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534706AC704").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasDefaultValue(2);
            entity.Property(e => e.Salt)
                .HasMaxLength(255)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");

            entity.HasMany(d => d.CarComponents).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserCarComponentSelection",
                    r => r.HasOne<CarComponent>().WithMany()
                        .HasForeignKey("CarComponentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserCarComponentSelection_CarComponent"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserCarComponentSelection_User"),
                    j =>
                    {
                        j.HasKey("UserId", "CarComponentId").HasName("PK__UserCarC__495C2FCAE270C3E7");
                        j.ToTable("UserCarComponentSelection");
                    });
        });
        modelBuilder.Entity<CarConfiguration>(entity =>
        {
            entity.ToTable("CarConfiguration");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasDefaultValue("Unnamed Configuration");
        });



        modelBuilder.Entity<CarConfigurationComponent>(entity =>
        {
            entity.ToTable("CarConfigurationComponent");

            entity.HasKey(cc => new { cc.CarConfigurationId, cc.CarComponentId });

            entity.HasOne(cc => cc.CarConfiguration)
                  .WithMany(c => c.CarConfigurationComponents)
                  .HasForeignKey(cc => cc.CarConfigurationId);

            entity.HasOne(cc => cc.CarComponent)
                  .WithMany()
                  .HasForeignKey(cc => cc.CarComponentId);
        });





        OnModelCreatingPartial(modelBuilder);
    }

     

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
//1