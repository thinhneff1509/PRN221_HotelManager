using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Threading.Tasks;

namespace Project_Group5.Pages.Admin.FeedbackManagement
{
    public class EditFeedbackModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public EditFeedbackModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Feedback Feedback { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Feedback = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.Room)
                    .ThenInclude(r => r.Roomtype)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (Feedback == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu ID của Feedback là hợp lệ
            if (Feedback.Id == 0)
            {
                return NotFound();
            }

            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra xem ID có hợp lệ hay không
            if (Feedback.Id == 0)
            {
                return NotFound();
            }

            _context.Attach(Feedback).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(Feedback.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Feedback");
        }


        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}
