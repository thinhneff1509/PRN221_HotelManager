namespace Project_Group5.Models
{
    public class SelectedRoom
    {
        public int RoomTypeId { get; set; }
        public int RoomId { get; set; }
        public string Name { get; set; }
        public string? RoomType { get; set; }
        public decimal Price { get; set; }
        public int AdultCount { get; set; }
        public int ChildrenCount { get; set; }
        public int Bed { get; set; }
    }
}