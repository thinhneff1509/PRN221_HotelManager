using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class Discount
    {
        public Discount()
        {
            Bookings = new HashSet<Booking>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public string? Amount { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
