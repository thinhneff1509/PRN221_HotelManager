using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.Admin.RoomManagement
{
    public class RoomtypeModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public RoomtypeModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public RoomType RoomType { get; set; }
        public List<RoomType> RoomTypes { get; set; }

        // Hiển thị danh sách phòng
        public async Task OnGetAsync(string searchString)
        {
            // Lấy tất cả các phòng từ bảng Room và tìm kiếm theo số phòng hoặc trạng thái phòng
            RoomTypes = await _context.RoomTypes
                .Include(r => r.Rooms)
                .Where(r => string.IsNullOrEmpty(searchString) ||
                            r.Name.Contains(searchString))
                .ToListAsync();
        }

        // Thêm mới phòng
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["ShowModal"] = true;
                RoomTypes = await _context.RoomTypes.ToListAsync();
                return Page(); // Nếu dữ liệu không hợp lệ, trở lại trang
            }

            _context.RoomTypes.Add(RoomType); // Thêm phòng mới vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách phòng
        }

        // Cập nhật phòng
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var roomTypeToUpdate = await _context.RoomTypes.FindAsync(RoomType.Id); // Tìm phòng theo ID
            if (roomTypeToUpdate == null)
            {
                return NotFound(); // Nếu không tìm thấy phòng
            }

            // Cập nhật thông tin phòng
            roomTypeToUpdate.Name = RoomType.Name;
            roomTypeToUpdate.Price = RoomType.Price;
            roomTypeToUpdate.Description = RoomType.Description;
            roomTypeToUpdate.Bed = RoomType.Bed;

            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách phòng
        }

        // Xóa hạng phòng
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var roomTypeToDelete = await _context.RoomTypes
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.Bookings)
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.Bookings)
                                                        .ThenInclude(b => b.Payments)
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.Bookings)
                                                        .ThenInclude(b => b.ServiceRegistrations)
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.ImageRooms)
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.Feedbacks)
                                                 .Include(rt => rt.Rooms)
                                                    .ThenInclude(r => r.Wishlists)
                                                 .FirstOrDefaultAsync(rt => rt.Id == id);

            if (roomTypeToDelete == null)
            {
                return NotFound(); // Nếu không tìm thấy hạng phòng
            }

            // Loop through each room and delete related entities
            foreach (var room in roomTypeToDelete.Rooms)
            {
                // Delete related ImageRooms
                if (room.ImageRooms != null)
                {
                    _context.ImageRooms.RemoveRange(room.ImageRooms);
                }

                // Delete related Feedbacks
                if (room.Feedbacks != null)
                {
                    _context.Feedbacks.RemoveRange(room.Feedbacks);
                }

                // Delete related Wishlists
                if (room.Wishlists != null)
                {
                    _context.Wishlists.RemoveRange(room.Wishlists);
                }

                // Delete related Bookings and their dependencies
                if (room.Bookings != null)
                {
                    foreach (var booking in room.Bookings)
                    {
                        if (booking.ServiceRegistrations != null)
                        {
                            _context.ServiceRegistrations.RemoveRange(booking.ServiceRegistrations);
                        }
                        if (booking.Payments != null)
                        {
                            _context.Payments.RemoveRange(booking.Payments);
                        }
                    }
                    _context.Bookings.RemoveRange(room.Bookings);
                }
            }

            // Delete the rooms themselves
            _context.Rooms.RemoveRange(roomTypeToDelete.Rooms);

            // Delete the RoomType
            _context.RoomTypes.Remove(roomTypeToDelete);

            // Save changes
            await _context.SaveChangesAsync();

            // Set success message
            TempData["SuccessMessage"] = "Xóa hạng phòng thành công!";

            return RedirectToPage();
        }

    }
}
