using System;
using System.Collections.Generic;

namespace AuthenHotel.Models
{
    public partial class ImageRoom
    {
        public int Id { get; set; }
        public string? Path { get; set; }
        public int? RoomId { get; set; }

        public virtual Room? Room { get; set; }
    }
}
