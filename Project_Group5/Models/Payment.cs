using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class Payment
    {
        public int Id { get; set; }
        public int? BookingId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Amount { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }

        public virtual Booking? Booking { get; set; }
    }
}
