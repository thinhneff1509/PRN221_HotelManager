using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class DetailModel : PageModel
    {
        public Hotel Hotel { get; set; }
        public List<Feedback> PreviousFeedbacks { get; set; }

        public void OnGet(int id)
        {
            // Simulate fetching hotel data from a database based on the given id
            Hotel = GetHotelById(id);
            PreviousFeedbacks = GetFeedbacksForHotel(id);
        }

        private Hotel GetHotelById(int id)
        {
            // Simulated data, replace with actual DB call
            return new Hotel
            {
                Id = id,
                Name = "Muong Thanh Grand Ha Long",
                ImageUrl = "/images/hotel1.jpg",
                Address = "phường Bãi Cháy, Ha Long City",
                PricePerNight = 1150000,
                Amenities = new[] { "Wi-Fi", "Pool", "Gym", "Spa" },
                Description = "Muong Thanh Grand Ha Long là lựa chọn tuyệt vời cho quý khách với phòng chức năng rộng lớn, được trang bị đầy đủ để sẵn sàng đáp ứng mọi yêu cầu.\r\n\r\nKhách sạn này là lựa chọn hoàn hảo cho các kỳ nghỉ mát lãng mạn hay tuần trăng mật của các cặp đôi. Quý khách hãy tận hưởng những đêm đáng nhớ nhất cùng người thương của mình tại Muong Thanh Grand Ha Long\r\n\r\nTừ sự kiện doanh nghiệp đến họp mặt công ty, Muong Thanh Grand Ha Long cung cấp đầy đủ các dịch vụ và tiện nghi đáp ứng mọi nhu cầu của quý khách và đồng nghiệp.\r\n\r\nHãy tận hưởng thời gian vui vẻ cùng cả gia đình với hàng loạt tiện nghi giải trí tại Muong Thanh Grand Ha Long, một khách sạn tuyệt vời phù hợp cho mọi kỳ nghỉ bên người thân.\r\n\r\nNếu dự định có một kỳ nghỉ dài, thì Muong Thanh Grand Ha Long chính là lựa chọn dành cho quý khách. Với đầy đủ tiện nghi với chất lượng dịch vụ tuyệt vời, Muong Thanh Grand Ha Long sẽ khiến quý khách cảm thấy thoải mái như đang ở nhà vậy.\r\n\r\nDu lịch một mình cũng không hề kém phần thú vị và Muong Thanh Grand Ha Long là nơi thích hợp dành riêng cho những ai đề cao sự riêng tư trong kỳ lưu trú.\r\n\r\nHãy sẵn sàng đón nhận trải nghiệm khó quên bằng dịch vụ độc đáo và hoàn hảo của khách sạn cùng các tiện nghi đầy đủ, đáp ứng mọi nhu cầu của quý khách.\r\n\r\nTrung tâm thể dục của khách sạn là một trong những tiện nghi không thể bỏ qua khi lưu trú tại đây.\r\n\r\nHưởng thụ một ngày thư thái đầy thú vị tại hồ bơi dù quý khách đang du lịch một mình hay cùng người thân.\r\n\r\nQuầy tiếp tân 24 giờ luôn sẵn sàng phục vụ quý khách từ thủ tục nhận phòng đến trả phòng hay bất kỳ yêu cầu nào. Nếu cần giúp đỡ xin hãy liên hệ đội ngũ tiếp tân, chúng tôi luôn sẵn sàng hỗ trợ quý khách.\r\n\r\nTận hưởng những món ăn yêu thích với phong cách ẩm thực đặc biệt từ Muong Thanh Grand Ha Long chỉ dành riêng cho quý khách.\r\n\r\nSóng WiFi phủ khắp các khu vực chung của khách sạn cho phép quý khách luôn kết nối với gia đình và bè bạn.\r\n\r\nTận hưởng trải nghiệm lưu trú xa hoa đầy thú vị không đâu sánh bằng tại Muong Thanh Grand Ha Long."
            };
        }

        private List<Feedback> GetFeedbacksForHotel(int hotelId)
        {
            // Simulate fetching feedback data from the database
            return new List<Feedback>
        {
            new Feedback { Rating = 5, Content = "Ấn tượng với khách sạn bởi sự chỉn chu và nhiệt tình trong cách nhân viên tạo ra trải nghiệm cho khách hàng. Đặc biệt có cảm tình với bạn Nam Anh, nhiệt tình hỗ trợ trong quá trình mình tổ chức event tại khách sạn. Chắc chắn sẽ recommand khách sạn này cho bạn bè!" },
            new Feedback { Rating = 4, Content = "Phòng ốc sạch sẽ, nhân viên phục vụ tận tình." },
            new Feedback { Rating = 3, Content = "Khách sạn ổn nhưng dịch vụ ăn uống chưa được tốt lắm." }
        };
        }

        public IActionResult OnPostSubmitFeedback(int rating, string comment)
        {
            // Save the rating and comment in the database or process as needed
            // You can redirect or show a success message after submission
            TempData["FeedbackMessage"] = "Cảm ơn bạn đã gửi đánh giá!";
            return RedirectToPage();
        }

    }

    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public decimal PricePerNight { get; set; }
        public string[] Amenities { get; set; }
        public string Description { get; set; }
    }
}
