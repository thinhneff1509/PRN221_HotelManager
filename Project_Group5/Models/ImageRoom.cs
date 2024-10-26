using System;
using System.Collections.Generic;

namespace Project_Group5.Models
{
    public partial class ImageRoom
    {
        public int Id { get; set; }
        public string? Path { get; set; }
        public int? RoomtypeId { get; set; }

        public virtual RoomType? Roomtype { get; set; }
    }
}
