using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PetShop_Project_SWP391.Pages.Manager.CustomerList
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly ProjectContext dBContext;
        private readonly IConfiguration configuration;

        public IndexModel(ProjectContext dBContext, IConfiguration configuration)
        {
            this.dBContext = dBContext;
            this.configuration = configuration;
        }

        [BindProperty]
        public List<Models.Account> Accounts { get; set; }
        [BindProperty]
        public List<Customer> Customers { get; set; }
        [BindProperty]
        public Models.Account Account { get; set; }
        public Employee Employee { get; set; }

        public List<bool?> StatusList { get; set; } // Danh sách trạng thái từ cơ sở dữ liệu
        public string CurrentStatus { get; set; } // Lưu trạng thái hiện tại
        public string CurrentSearch { get; set; } // Lưu giá trị tìm kiếm hiện tại

        public async Task<IActionResult> OnGet(int? PageNum, string? txtSearch, string? statusFilter)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/singnin");
            }

            if (PageNum <= 0 || PageNum is null) PageNum = 1;

            int PageSize = Convert.ToInt32(configuration.GetValue<string>("AppSettings:PageSize"));
            PageSize = PageSize == 0 ? 8 : PageSize; // Giá trị mặc định

            // Lấy danh sách trạng thái từ cơ sở dữ liệu
            StatusList = await dBContext.Accounts.Select(a => a.IsActive).Distinct().ToListAsync();

            Accounts = await dBContext.Accounts
                .Where(x => x.Role == 2)
                .Include(x => x.Customer)
                .ToListAsync();
            //search only
            if (!string.IsNullOrEmpty(txtSearch))
            {
                Accounts = Accounts
                    .Where(x =>
                        x.Customer.Name.IndexOf(txtSearch.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        x.Customer.Phone.IndexOf(txtSearch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();
            }
            //filter only
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter.ToLower() != "tất cả")
            {
                bool? filterStatus = bool.Parse(statusFilter); // Chuyển đổi chuỗi thành giá trị bool
                Accounts = Accounts.Where(x => x.IsActive == filterStatus).ToList();
                CurrentStatus = bool.Parse(statusFilter) ? "Hoạt Động" : "Khóa";
            }
            //filter and search 
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter.ToLower() != "tất cả" && !string.IsNullOrEmpty(txtSearch))
            {
                bool? filterStatus = bool.Parse(statusFilter); // Chuyển đổi chuỗi thành giá trị bool
                Accounts = Accounts
                .Where(x =>
                (string.IsNullOrEmpty(txtSearch) ||
                x.Customer.Name.IndexOf(txtSearch.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                x.Customer.Phone.IndexOf(txtSearch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrEmpty(statusFilter) ||
                x.IsActive == filterStatus)
                )
                .ToList();
                CurrentStatus = bool.Parse(statusFilter) ? "Hoạt Động" : "Khóa";
            }
            int TotalProduct = Accounts.Count;
            int TotalPage = PageSize > 0 ? (TotalProduct / PageSize) + (TotalProduct % PageSize == 0 ? 0 : 1) : 0;

            ViewData["TotalPage"] = TotalPage;
            ViewData["CurPage"] = PageNum;
            ViewData["txtSearch"] = txtSearch;

            Accounts = Accounts.Skip((int)(((PageNum - 1) * PageSize + 1) - 1)).Take(PageSize).ToList();

            return Page();
        }

        public IActionResult OnGetActive(int? id)
        {
            if (id == null) return Redirect("~/Manager/CustomerList/index"); 

            Account = dBContext.Accounts.FirstOrDefault(x => x.AccountId == id);
            if (Account == null) return Redirect("~/Manager/CustomerList/index");

            Account.IsActive = !Account.IsActive;
            dBContext.SaveChanges();

            return Redirect("~/Manager/CustomerList/index");
        }
    }
}
