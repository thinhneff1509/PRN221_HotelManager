using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Dto;
using Project_Group5.Models;

namespace Project_Group5.Pages.Admin.RoomManagement
{
    public class IndexModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        /*        [BindProperty]
                public Room Room { get; set; }*/
        [BindProperty]
        public RoomDto Room { get; set; }
        public List<Room> Rooms { get; set; }
        public List<RoomType> RoomTypes { get; set; }

        // Hiển thị danh sách phòng
        public async Task OnGetAsync(string searchString)
        {
            // Lấy tất cả các phòng từ bảng Room và tìm kiếm theo số phòng hoặc trạng thái phòng
            Rooms = await _context.Rooms
                .Where(r => string.IsNullOrEmpty(searchString) ||
                            r.RoomNumber.Contains(searchString) ||
                            r.Status.Contains(searchString))
                .Include(r => r.Roomtype) // Bao gồm RoomType để hiển thị loại phòng
                .Include(r => r.ImageRooms)
                .ToListAsync();

            // Lấy danh sách các loại phòng
            RoomTypes = await _context.RoomTypes.ToListAsync();
        }

        // Thêm mới phòng
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                RoomTypes = await _context.RoomTypes.ToListAsync();
                ViewData["ShowModal"] = true;
                Rooms = await _context.Rooms.Include(r => r.Roomtype).ToListAsync();
                return Page(); // Nếu dữ liệu không hợp lệ, trở lại trang
            }
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == Room.RoomNumber))
            {
                RoomTypes = await _context.RoomTypes.ToListAsync();
                ViewData["ShowModal"] = true;
                Rooms = await _context.Rooms.Include(r => r.Roomtype).ToListAsync();
                TempData["Error"] = "Phòng đã tồn tại";
                return Page();
            }
            var room = new Room
            {
                RoomNumber = Room.RoomNumber,
                Status = Room.Status,
                RoomtypeId = Room.RoomtypeId
            };

            _context.Rooms.Add(room); // Thêm phòng mới vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            if (Room.ImageFiles != null && Room.ImageFiles.Count > 0)
            {
                foreach (var file in Room.ImageFiles)
                {
                    var imageRoom = new ImageRoom
                    {
                        RoomId = room.Id,
                        Path = await SaveImageFileAsync(file) // Save the file and get the path
                    };
                    _context.ImageRooms.Add(imageRoom);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(); // Chuyển hướng về trang danh sách phòng
        }

        // Cập nhật phòng
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var roomToUpdate = await _context.Rooms.FindAsync(Room.Id); // Tìm phòng theo ID
            if (roomToUpdate == null)
            {
                return NotFound(); // Nếu không tìm thấy phòng
            }

            // Cập nhật thông tin phòng
            roomToUpdate.RoomNumber = Room.RoomNumber;
            roomToUpdate.Status = Room.Status;
            roomToUpdate.RoomtypeId = Room.RoomtypeId;

            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            if (Room.ImageFiles != null && Room.ImageFiles.Count > 0)
            {
                foreach (var file in Room.ImageFiles)
                {
                    var imageRoom = new ImageRoom
                    {
                        RoomId = roomToUpdate.Id,
                        Path = await SaveImageFileAsync(file)
                    };
                    _context.ImageRooms.Add(imageRoom);
                }
                await _context.SaveChangesAsync();
            }


            return RedirectToPage(); // Chuyển hướng về trang danh sách phòng
        }

        // Xóa phòng
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var roomToDelete = await _context.Rooms
                                             .Include(r => r.Bookings)
                                                 .ThenInclude(b => b.Payments)
                                             .Include(r => r.Bookings)
                                                 .ThenInclude(b => b.ServiceRegistrations)
                                             .Include(r => r.ImageRooms)
                                             .Include(r => r.Feedbacks)
                                             .Include(r => r.Wishlists)  // Include Wishlists liên quan
                                             .FirstOrDefaultAsync(r => r.Id == id);

            if (roomToDelete == null)
            {
                return NotFound(); // Nếu không tìm thấy phòng
            }

            // Xóa các bản ghi liên quan trong ImageRooms
            if (roomToDelete.ImageRooms != null)
            {
                _context.ImageRooms.RemoveRange(roomToDelete.ImageRooms);
            }

            // Xóa các bản ghi liên quan trong Feedbacks
            if (roomToDelete.Feedbacks != null)
            {
                _context.Feedbacks.RemoveRange(roomToDelete.Feedbacks);
            }

            // Xóa các bản ghi liên quan trong Wishlists
            if (roomToDelete.Wishlists != null)
            {
                _context.Wishlists.RemoveRange(roomToDelete.Wishlists);
            }

            // Xóa các bản ghi liên quan trong Bookings và các phụ thuộc của nó
            if (roomToDelete.Bookings != null)
            {
                foreach (var booking in roomToDelete.Bookings)
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
                _context.Bookings.RemoveRange(roomToDelete.Bookings);
            }

            // Xóa Room
            _context.Rooms.Remove(roomToDelete);
            await _context.SaveChangesAsync();

            // Sử dụng TempData để lưu trữ thông báo
            TempData["SuccessMessage"] = "Xóa phòng thành công!";

            return RedirectToPage();
        }

        private async Task<string> SaveImageFileAsync(IFormFile file)
        {
            string uploadFolder = "wwwroot/images/rooms";
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var filePath = Path.Combine(uploadFolder, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return "rooms/" + Path.GetFileName(filePath); // Return relative path
        }

    }
}

