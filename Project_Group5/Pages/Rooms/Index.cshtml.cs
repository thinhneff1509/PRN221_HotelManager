using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public IList<RoomType> Rooms { get; set; }

        public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }
        public async Task OnGetAsync()
        {
            // Fetch all rooms from the database
            Rooms = await _context.RoomTypes.Include(r => r.ImageRooms).Include(r => r.Rooms).ToListAsync();
        }
    }
}
