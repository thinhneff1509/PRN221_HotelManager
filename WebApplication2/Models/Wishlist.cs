using System;
using System.Collections.Generic;

namespace HotelManagement.Models
{
    public partial class Wishlist
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? RoomId { get; set; }
        public DateTime? FavouriteDate { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Room? Room { get; set; }
    }
}
