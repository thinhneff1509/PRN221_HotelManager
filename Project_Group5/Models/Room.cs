using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class Room
    {
        public Room()
        {
            Bookings = new HashSet<Booking>();
            Feedbacks = new HashSet<Feedback>();
            Wishlists = new HashSet<Wishlist>();
        }

        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public int Status { get; set; }
        public int RoomtypeId { get; set; }

        public virtual RoomType Roomtype { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}
