using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System.Text.Json;

namespace PetShop_Project_SWP391.Pages.Manager.Products
{
    public class CreateModel : PageModel
    {
        //private readonly ProjectContext _context;

        //public CreateModel(ProjectContext context)
        //{
        //    _context = context;
        //}
        //[BindProperty]
        //public Models.Product Product { get; set; }
        //[BindProperty]
        //public Models.PictureProduct1 pictureProduct { get; set; }
        //[BindProperty]
        //public Models.Account Account { get; set; }
        //[BindProperty]
        //public List<Models.Category> Category { get; set; }
        //[BindProperty]
        //public Models.Employee Employee { get; set; }
        //public async Task<IActionResult> OnGet()
        //{
        //    if (HttpContext.Session.GetString("PetSession") == null)
        //        return RedirectToPage("/account/singnin");

        //    Account = JsonSerializer.Deserialize<Models.Account>(HttpContext.Session.GetString("PetSession"));
        //    Employee = _context.Employees.FirstOrDefault(c => c.EmployeeId == Account.EmployeeId);
        //    Category = await _context.Categories.ToListAsync();
        //    return Page();
        //}
        //public async Task<IActionResult> OnPost(IFormFile imageFile)
        //{
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        Console.WriteLine(imageFile);
        //        pictureProduct.Picture = "/img/" + imageFile.FileName.ToString();

        //        //dBContext.BlogDetails.Add(BlogDetail);

        //    }
        //    var product = new Models.Product()
        //    {
        //        ProductName = Product.ProductName,
        //        QuantityPerUnit = Product.QuantityPerUnit,
        //        UnitPrice = Product.UnitPrice,
        //        Description = Product.Description,
        //        CreateDate = DateTime.Now,
        //        CategoryId = Product.CategoryId,
        //        Pictures= Product.Pictures,
        //        Status = true,
        //        Discontinued = false
        //    };
        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();
        //    ViewData["msgSuccess"] = "Thêm sản phẩm thành công";
        //    return RedirectToPage("/Manager/Products/Index");
        //}
        private readonly ProjectContext _context;

        public CreateModel(ProjectContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Product Product { get; set; }

        [BindProperty]
        public Models.PictureProduct1 pictureProduct { get; set; }

        [BindProperty]
        public Models.Account Account { get; set; }

        [BindProperty]
        public List<Models.Category> Category { get; set; }

        [BindProperty]
        public Models.Employee Employee { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (HttpContext.Session.GetString("PetSession") == null)
                return RedirectToPage("/account/singnin");

            Account = JsonSerializer.Deserialize<Models.Account>(HttpContext.Session.GetString("PetSession"));
            Employee = _context.Employees.FirstOrDefault(c => c.EmployeeId == Account.EmployeeId);
            Category = await _context.Categories.ToListAsync();
            return Page();
        }

        //public async Task<IActionResult> OnPost(IFormFile imageFile)
        //{
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        // Lưu tập tin hình ảnh vào thư mục "wwwroot/img"
        //        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", imageFile.FileName);
        //        using (var stream = new FileStream(imagePath, FileMode.Create))
        //        {
        //            await imageFile.CopyToAsync(stream);
        //        }

        //        // Đặt thuộc tính Picture của pictureProduct thành đường dẫn của hình ảnh đã lưu
        //        pictureProduct.Picture = "/img/" + imageFile.FileName;
        //    }

        //    var product = new Models.Product()
        //    {
        //        ProductName = Product.ProductName,
        //        QuantityPerUnit = Product.QuantityPerUnit,
        //        UnitPrice = Product.UnitPrice,
        //        Description = Product.Description,
        //        CreateDate = DateTime.Now,
        //        CategoryId = Product.CategoryId,
        //        Pictures = Product.Pictures,
        //        Status = true,
        //        Discontinued = false
        //    };

        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();

        //    ViewData["msgSuccess"] = "Thêm sản phẩm thành công";
        //    return RedirectToPage("/Manager/Products/Index");
        //}
        public async Task<IActionResult> OnPost(string imageLink)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageLink))
                {
                    int maxLength = 200; // Adjust the maximum length as needed
                    pictureProduct.Picture = imageLink.Length <= maxLength ? imageLink : imageLink.Substring(0, maxLength);
                }
                //pictureProduct.PictureId = 0;
                var product = new Models.Product()
                {
                    ProductName = Product.ProductName,
                    QuantityPerUnit = Product.QuantityPerUnit,
                    UnitPrice = Product.UnitPrice,
                    Description = Product.Description,
                    CreateDate = DateTime.Now,
                    CategoryId = Product.CategoryId,
                    Pictures = new List<PictureProduct1> { pictureProduct }, // Add the pictureProduct to the list of pictures
                    Status = true,
                    Discontinued = false
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                ViewData["msgSuccess"] = "Thêm sản phẩm thành công";
                return RedirectToPage("/Manager/Products/Index");
            }catch(Exception e)
            {
                return RedirectToPage("/Manager/Products/Index");
            }
        }
    }
}
