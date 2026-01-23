using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class RealEstateListingPlatformContext : DbContext
{
    public RealEstateListingPlatformContext()
    {
    }

    public RealEstateListingPlatformContext(DbContextOptions<RealEstateListingPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingMedium> ListingMedia { get; set; }

    public virtual DbSet<ListingPriceHistory> ListingPriceHistories { get; set; }

    public virtual DbSet<ListingTour360> ListingTour360s { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

<<<<<<< Updated upstream
//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer(GetConnectionString());

//    private string GetConnectionString()
//    {
//        IConfiguration config = new ConfigurationBuilder()
//             .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("appsettings.json", true, true)
//                    .Build();
//        var strConn = config["ConnectionStrings:DefaultConnection"];

//        return strConn;
//    }
=======
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer(GetConnectionString());

    //    private string GetConnectionString()
    //    {
    //        IConfiguration config = new ConfigurationBuilder()
    //             .SetBasePath(Directory.GetCurrentDirectory())
    //                    .AddJsonFile("appsettings.json", true, true)
    //                    .Build();
    //        var strConn = config["ConnectionStrings:DefaultConnection"];

    //        return strConn;
    //    }
>>>>>>> Stashed changes
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC076A693CD4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.TargetType).HasMaxLength(50);

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ListingId }).HasName("PK__Favorite__0C7B27A1ABB4CD77");

            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Listing).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Listings");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Favorites_Users");
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leads__3214EC0776E33481");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ListerNote).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("New");

            entity.HasOne(d => d.Lister).WithMany(p => p.LeadListers)
                .HasForeignKey(d => d.ListerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leads_Lister");

            entity.HasOne(d => d.Listing).WithMany(p => p.Leads)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK_Leads_Listings");

            entity.HasOne(d => d.Seeker).WithMany(p => p.LeadSeekers)
                .HasForeignKey(d => d.SeekerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leads_Seeker");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Listings__3214EC0776BA7CA4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Area).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Direction).HasMaxLength(20);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.FurnitureStatus).HasMaxLength(50);
            entity.Property(e => e.HouseNumber).HasMaxLength(50);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LegalStatus).HasMaxLength(50);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PropertyType).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.StreetName).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ward).HasMaxLength(50);

            entity.HasOne(d => d.Lister).WithMany(p => p.Listings)
                .HasForeignKey(d => d.ListerId)
                .HasConstraintName("FK_Listings_Users");
        });

        modelBuilder.Entity<ListingMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ListingM__3214EC07052EBA9D");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.MediaType).HasMaxLength(10);

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingMedia)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK_ListingMedia_Listings");
        });

        modelBuilder.Entity<ListingPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ListingP__3214EC07CA96AC2A");

            entity.ToTable("ListingPriceHistory");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.ListingPriceHistories)
                .HasForeignKey(d => d.ChangedByUserId)
                .HasConstraintName("FK_PriceHistory_Users");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingPriceHistories)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK_PriceHistory_Listings");
        });

        modelBuilder.Entity<ListingTour360>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__ListingT__BF3EBED013672435");

            entity.ToTable("ListingTour360");

            entity.Property(e => e.ListingId).ValueGeneratedNever();
            entity.Property(e => e.Provider).HasMaxLength(50);

            entity.HasOne(d => d.Listing).WithOne(p => p.ListingTour360)
                .HasForeignKey<ListingTour360>(d => d.ListingId)
                .HasConstraintName("FK_ListingTour360_Listings");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC078D8008EF");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.RelatedLink).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reports__3214EC0709C5B507");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdminResponse).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Listing).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK_Reports_Listings");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reports_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07151CC5C6");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105347CE7345E").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.LastLoginAt).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
