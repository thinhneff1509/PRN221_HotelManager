using System;
using System.Collections.Generic;

namespace AuthenHotel.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Bookings = new HashSet<Booking>();
            Feedbacks = new HashSet<Feedback>();
            Wishlists = new HashSet<Wishlist>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime? Dob { get; set; }
        public string Phone { get; set; } = null!;
        public string? Username { get; set; }
        public string Password { get; set; } = null!;
        public string? Address { get; set; }
        public DateTime? RegisterDate { get; set; }
        public int? RoleId { get; set; }
        public bool? Banned { get; set; }

        public virtual Role? Role { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}
