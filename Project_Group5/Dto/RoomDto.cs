using Microsoft.AspNetCore.Mvc;

namespace Project_Group5.Dto
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int RoomtypeId { get; set; }
        [BindProperty]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
