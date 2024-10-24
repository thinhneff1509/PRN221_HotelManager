using System;
using System.Collections.Generic;

namespace AuthenHotel.Models
{
    public partial class Booking
    {
        public Booking()
        {
            Discounts = new HashSet<Discount>();
            Payments = new HashSet<Payment>();
            ServiceRegistrations = new HashSet<ServiceRegistration>();
        }

        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? RoomId { get; set; }
        public string? Status { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? PaymentStatus { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Room? Room { get; set; }
        public virtual ICollection<Discount> Discounts { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<ServiceRegistration> ServiceRegistrations { get; set; }
    }
}
