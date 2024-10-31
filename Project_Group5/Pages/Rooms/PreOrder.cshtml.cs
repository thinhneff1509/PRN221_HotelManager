using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class PreOrderModel : PageModel
    {
        [BindProperty]
        public string CheckinDate { get; set; }

        [BindProperty]
        public string CheckoutDate { get; set; }

        [BindProperty]
        public string RoomData { get; set; }
        [BindProperty]
        public int StayDuration { get; set; }
        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Dob { get; set; }
        //public List<RoomData> SelectedRooms { get; set; }

        public void OnGet(string checkinDate, string checkoutDate, string roomData)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            RoomData = roomData;
            if (DateOnly.TryParse(CheckinDate, out var checkInDate) && DateOnly.TryParse(CheckoutDate, out var checkOutDate))
            {
                TimeSpan dateDifference = checkOutDate.ToDateTime(new TimeOnly()) - checkInDate.ToDateTime(new TimeOnly());

                StayDuration = dateDifference.Days;
            }
            if (roomData != null)
            {
                Console.WriteLine(roomData);
                // Deserialize RoomData JSON into a list of RoomData objects
                //SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(RoomData);
            }
        }

        public void OnPost() { }
    }
}
