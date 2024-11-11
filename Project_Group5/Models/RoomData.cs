﻿namespace Project_Group5.Models
{
    public class RoomData
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int Bed { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public int? AvailableRoom { get; set; }
        public List<SelectedRoom> RoomList { get; set; }
    }
}