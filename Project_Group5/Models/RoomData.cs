namespace Project_Group5.Models
{
    public class RoomData
    {
        public string RoomType { get; set; }
        public int Bed { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public List<PreOrderRoom> RoomList { get; set; }
    }
}

