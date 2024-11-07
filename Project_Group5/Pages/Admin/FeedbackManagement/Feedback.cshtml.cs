using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project_Group5.Models;

namespace Project_Group5.Pages.Admin.FeedbackManagement
{
    public class FeedbackModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public FeedbackModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public List<Feedback> FeedbackList { get; set; }

        public async Task OnGetAsync()
        {
            FeedbackList = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Room)
                    .ThenInclude(r => r.Roomtype)
                .ToListAsync();
        }


        // Delete method
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

    }
}