using System;
using System.Collections.Generic;

namespace AuthenHotel.Models
{
    public partial class Discount
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public int? BookingId { get; set; }

        public virtual Booking? Booking { get; set; }
    }
}
