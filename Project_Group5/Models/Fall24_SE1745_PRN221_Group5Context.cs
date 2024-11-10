using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Project_Group5.Models
{
    public partial class Fall24_SE1745_PRN221_Group5Context : DbContext
    {
        public Fall24_SE1745_PRN221_Group5Context()
        {
        }

        public Fall24_SE1745_PRN221_Group5Context(DbContextOptions<Fall24_SE1745_PRN221_Group5Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Feedback> Feedbacks { get; set; } = null!;
        public virtual DbSet<ImageRoom> ImageRooms { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<RoomType> RoomTypes { get; set; } = null!;
        public virtual DbSet<Service> Services { get; set; } = null!;
        public virtual DbSet<ServiceRegistration> ServiceRegistrations { get; set; } = null!;
        public virtual DbSet<Wishlist> Wishlists { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Booking");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CheckInDate)
                    .HasColumnType("date")
                    .HasColumnName("check_in_date");

                entity.Property(e => e.CheckOutDate)
                    .HasColumnType("date")
                    .HasColumnName("check_out_date");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.DiscountId).HasColumnName("discount_id");

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(255)
                    .HasColumnName("payment_status");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
                    .HasColumnName("status");

                entity.Property(e => e.TotalAmount)
                    .HasMaxLength(255)
                    .HasColumnName("total_amount");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Booking__custome__3D5E1FD2");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK_Booking_Discount");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK__Booking__room_id__3E52440B");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");

                entity.HasIndex(e => e.Email, "UQ__Customer__AB6E61649A1DECBB")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.Banned).HasColumnName("banned");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(255)
                    .HasColumnName("phone");

                entity.Property(e => e.RegisterDate)
                    .HasColumnType("date")
                    .HasColumnName("register_date");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .HasColumnName("username");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_Customer_Role");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasMaxLength(255)
                    .HasColumnName("amount");

                entity.Property(e => e.Content)
                    .HasMaxLength(255)
                    .HasColumnName("content");

                entity.Property(e => e.EffectiveDate)
                    .HasColumnType("date")
                    .HasColumnName("effective_date");

                entity.Property(e => e.ExpirationDate)
                    .HasColumnType("date")
                    .HasColumnName("expiration_date");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Content)
                    .HasMaxLength(500)
                    .HasColumnName("content");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.FeedbackDate)
                    .HasColumnType("date")
                    .HasColumnName("feedback_date");

                entity.Property(e => e.Rating).HasColumnName("rating");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Feedback__custom__49C3F6B7");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK_Feedback_Room");
            });

            modelBuilder.Entity<ImageRoom>(entity =>
            {
                entity.ToTable("ImageRoom");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Path)
                    .HasMaxLength(255)
                    .HasColumnName("path");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.ImageRooms)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK_ImageRoom_Room");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasMaxLength(255)
                    .HasColumnName("amount");

                entity.Property(e => e.BookingId).HasColumnName("booking_id");

                entity.Property(e => e.CheckIn)
                    .HasColumnType("date")
                    .HasColumnName("check_in");

                entity.Property(e => e.CheckOut)
                    .HasColumnType("date")
                    .HasColumnName("check_out");

                entity.Property(e => e.PaymentDate)
                    .HasColumnType("date")
                    .HasColumnName("payment_date");

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(255)
                    .HasColumnName("payment_method");

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
                    .HasColumnName("status");

                entity.HasOne(d => d.Booking)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.BookingId)
                    .HasConstraintName("FK__Payment__booking__4CA06362");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");

                entity.HasIndex(e => e.RoomNumber, "UQ__Room__FE22F61B295D1E22")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.RoomNumber)
                    .HasMaxLength(255)
                    .HasColumnName("room_number");

                entity.Property(e => e.RoomtypeId).HasColumnName("roomtype_id");

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
                    .HasColumnName("status");

                entity.HasOne(d => d.Roomtype)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.RoomtypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Room_RoomType");
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.ToTable("RoomType");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Bed).HasColumnName("bed");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Price).HasColumnName("price");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ImageService)
                    .HasMaxLength(255)
                    .HasColumnName("Image_service");

                entity.Property(e => e.Price)
                    .HasMaxLength(255)
                    .HasColumnName("price");

                entity.Property(e => e.ServiceName)
                    .HasMaxLength(255)
                    .HasColumnName("service_name");

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
                    .HasColumnName("status");
            });

            modelBuilder.Entity<ServiceRegistration>(entity =>
            {
                entity.HasKey(e => e.RegistrationId)
                    .HasName("PK__Service___22A298F639EEA133");

                entity.ToTable("Service_Registration");

                entity.Property(e => e.RegistrationId).HasColumnName("registration_id");

                entity.Property(e => e.BookingId).HasColumnName("booking_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.ServiceId).HasColumnName("service_id");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("money")
                    .HasColumnName("total_price");

                entity.HasOne(d => d.Booking)
                    .WithMany(p => p.ServiceRegistrations)
                    .HasForeignKey(d => d.BookingId)
                    .HasConstraintName("FK__Service_R__booki__4316F928");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServiceRegistrations)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("FK__Service_R__servi__440B1D61");
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.ToTable("Wishlist");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.FavouriteDate)
                    .HasColumnType("date")
                    .HasColumnName("favouriteDate");

                entity.Property(e => e.RoomId).HasColumnName("room_id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Wishlist_Customer");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Wishlists)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK_Wishlist_Room");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
