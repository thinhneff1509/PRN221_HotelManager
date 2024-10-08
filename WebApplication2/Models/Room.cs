using System;
using System.Collections.Generic;

namespace HotelManagement.Models
{
    public partial class Room
    {
        public Room()
        {
            Bookings = new HashSet<Booking>();
            Feedbacks = new HashSet<Feedback>();
            ImageRooms = new HashSet<ImageRoom>();
            Wishlists = new HashSet<Wishlist>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string? RoomType { get; set; }
        public string Price { get; set; } = null!;
        public int Status { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<ImageRoom> ImageRooms { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}
