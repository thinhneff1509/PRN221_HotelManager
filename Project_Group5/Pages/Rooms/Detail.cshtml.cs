using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class DetailModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public DetailModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }


        public RoomType RoomType { get; set; }
        public List<ImageRoom> ImageRoom { get; set; }
        public List<Feedback> Feedbacks { get; set; }

        public void OnGet(int id)
        {
            Room Room = _context.Rooms
                           .Include(r => r.Roomtype)
                           .Include(r => r.ImageRooms)
                           .FirstOrDefault(r => r.Id == id);

            if (Room != null)
            {
                RoomType = Room.Roomtype;
                ImageRoom = Room.ImageRooms.ToList();
            }
            else
            {
                // Handle not found case
                RedirectToPage("/NotFound"); // or return NotFound();
            }
        }

        public IActionResult OnPostBookRoom(int roomId)
        {
            return RedirectToAction("Book", "Booking", new { roomId });
        }

        
    }
}