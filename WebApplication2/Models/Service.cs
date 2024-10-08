using System;
using System.Collections.Generic;

namespace HotelManagement.Models
{
    public partial class Service
    {
        public Service()
        {
            ServiceRegistrations = new HashSet<ServiceRegistration>();
        }

        public int Id { get; set; }
        public string? ServiceName { get; set; }
        public string Price { get; set; } = null!;
        public string? Status { get; set; }

        public virtual ICollection<ServiceRegistration> ServiceRegistrations { get; set; }
    }
}
