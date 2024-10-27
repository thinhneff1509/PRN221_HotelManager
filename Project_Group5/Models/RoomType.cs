using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class RoomType
    {
        public RoomType()
        {
            Rooms = new HashSet<Room>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Bed { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }
    }
}
