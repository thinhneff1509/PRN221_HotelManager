using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.ComponentModel.DataAnnotations;

namespace Project_Group5.Pages.Rooms
{
    public class FeedbackModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        [BindProperty]
        public Feedback Feedback { get; set; }
        public List<Feedback> FeedbackList { get; set; }
        public FeedbackStats Stats { get; set; }

        public FeedbackModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public class FeedbackStats
        {
            public double AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public Dictionary<string, int> RatingBreakdown { get; set; }
            public Dictionary<string, double> CategoryRatings { get; set; }
        }

        public string CustomerName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == User.Identity.Name);

            CustomerName = customer?.Name ?? "Khách hàng";

            FeedbackList = await _context.Feedbacks
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackDate)
                .Take(10)
                .ToListAsync();

            var allFeedbacks = await _context.Feedbacks.ToListAsync();
            Stats = new FeedbackStats
            {
                TotalReviews = allFeedbacks.Count,
                AverageRating = (double)(allFeedbacks.Any() ? allFeedbacks.Average(f => f.Rating) : 0),
                RatingBreakdown = new Dictionary<string, int>
        {
            { "Tuyệt vời", allFeedbacks.Count(f => f.Rating == 5) },
            { "Rất tốt", allFeedbacks.Count(f => f.Rating == 4) },
            { "Hài lòng", allFeedbacks.Count(f => f.Rating == 3) },
            { "Trung bình", allFeedbacks.Count(f => f.Rating == 2) },
            { "Kém", allFeedbacks.Count(f => f.Rating == 1) }
        }
            };

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            Feedback.FeedbackDate = DateTime.Now;
            _context.Feedbacks.Add(Feedback);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}