using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System.Text.Json;

namespace PetShop_Project_SWP391.Pages.Manager.Products
{

    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly ProjectContext _context;
        private readonly IConfiguration configuration;
        public IndexModel(ProjectContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;
        }
        public List<Models.Product> Products { get; set; } = default!;
        [BindProperty]
        public @Models.Account Account { get; set; }
        [BindProperty]
        public PictureProduct1 pictureProduct { get; set; }
        [BindProperty]
        public Customer Customer { get; set; }
        [BindProperty]
        public Category Categories { get; set; }
        public List<Category> CategoryList { get; set; }

        public string importErr { get; set; }
        public async Task<IActionResult> OnGet(int? PageNum, string? txtSearch,int? categoryId)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/singnin");
            }
            else
            {
                if (PageNum <= 0 || PageNum is null) PageNum = 1;
                int PageSize = Convert.ToInt32(configuration.GetValue<string>("AppSettings:PageSize"));

                // Kiểm tra giá trị PageSize và gán giá trị mặc định nếu nó bằng 0
                if (PageSize == 0)
                {
                    PageSize = 10; // Giá trị mặc định
                }
                CategoryList = await _context.Categories.ToListAsync();
                Products = await _context.Products.Include(x => x.Category)
                    .Include(x=>x.Pictures).ToListAsync();
                if (txtSearch != null)
                {
                    string searchKeyword = txtSearch.Trim().ToLower();
                    Products = Products.Where(x => x.ProductName.ToLower().Contains(searchKeyword)).ToList();
                }
                if(categoryId != null)
                {
                    Products = _context.Products.Include(x=>x.Category).Where( x => x.CategoryId.Equals(categoryId)).ToList();
                }
                if(txtSearch != null && categoryId is not null) {
                    string searchKeyword = txtSearch.Trim().ToLower();
                    Products = _context.Products
                    .Include(x => x.Category)
                    .Where(x =>
                    x.ProductName.ToLower().Contains(searchKeyword) &&
                    (categoryId == null || categoryId.Equals(x.CategoryId))
                    )
                    .ToList();

                }

                int TotalProduct = Products.Count;

                // Kiểm tra PageSize để đảm bảo không chia cho 0
                int TotalPage = PageSize > 0 ? (TotalProduct / PageSize) : 0;
                if (TotalProduct % PageSize != 0) TotalPage++;
                ViewData["TotalPage"] = TotalPage;
                ViewData["CurPage"] = PageNum;
                ViewData["txtSearch"] = txtSearch;

                Products = Products.Skip((int)(((PageNum - 1) * PageSize + 1) - 1)).Take(PageSize).ToList();

                return Page();
            }
        }
        public IActionResult OnGetActive(int? id)
        {
            if (id == null) return Redirect("~/Manager/Products/index");

            Models.Product product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null) return Redirect("~/Manager/Products/index");

            if (product.Status == true)
            {
                product.Status = false;
                product.Discontinued = true;
                //product.QuantityPerUnit = 0;
            }
            else
            {
                product.Status = true;
                product.Discontinued = false;
            }
            _context.SaveChanges();

            return Redirect("~/Manager/Products/index");
        }
    }
}
