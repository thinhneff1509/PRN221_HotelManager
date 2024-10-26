using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class Service
    {
        public Service()
        {
            ServiceRegistrations = new HashSet<ServiceRegistration>();
        }

        public int Id { get; set; }
        public string? ServiceName { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }

        public virtual ICollection<ServiceRegistration> ServiceRegistrations { get; set; }
    }
}
