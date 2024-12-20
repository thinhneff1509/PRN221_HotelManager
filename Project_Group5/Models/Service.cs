﻿using System;
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
        public string Price { get; set; } = null!;
        public string? Status { get; set; }
        public string? ImageService { get; set; }

        public virtual ICollection<ServiceRegistration> ServiceRegistrations { get; set; }
    }
}
