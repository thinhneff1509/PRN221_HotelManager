using System;
using System.Collections.Generic;

namespace AuthenHotel.Models
{
    public partial class Payment
    {
        public int Id { get; set; }
        public int? BookingId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Amount { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public virtual Booking? Booking { get; set; }
    }
}
