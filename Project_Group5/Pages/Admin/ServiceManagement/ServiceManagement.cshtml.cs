using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models; // Thay bằng namespace của bạn
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Project_Group5.Pages.Admin.ServiceManagement
{
    public class ServiceManagementModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public ServiceManagementModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Service> Services { get; set; } = new List<Service>();

        [BindProperty]
        public string ServiceName { get; set; }

        [BindProperty]
        public string Price { get; set; }

        [BindProperty]
        public string Status { get; set; }

        public async Task OnGetAsync()
        {
            Services = await _context.Services.ToListAsync();
            Rooms = await _context.Rooms
        .Include(r => r.Roomtype)
        .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            var service = new Service
            {
                ServiceName = ServiceName,
                Price = Price,
                Status = Status
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string serviceName, string price, string status)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.ServiceName = serviceName;
                service.Price = price;
                service.Status = status;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
