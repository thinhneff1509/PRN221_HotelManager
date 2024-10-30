using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class ServiceRegistration
    {
        public int RegistrationId { get; set; }
        public int? BookingId { get; set; }
        public int? ServiceId { get; set; }
        public int Quantity { get; set; }
        public string? TotalPrice { get; set; }

        public virtual Booking? Booking { get; set; }
        public virtual Service? Service { get; set; }
    }
}
