    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using PetShop_Project_SWP391.Models;
    using System;
    using System.Data;
    using System.Text.Json;

    namespace PetShop_Project_SWP391.Pages.Manager.Products
    {
        [Authorize(Roles = "1")]
        public class UpdateModel : PageModel
        {
            private readonly ProjectContext _context;
            public UpdateModel(ProjectContext context) => _context = context;

                [BindProperty]
                public Models.PictureProduct1 pictureProduct { get; set; }

            [BindProperty]
            public Models.Product Product { get; set; }
            [BindProperty]
            public @Models.Account Account { get; set; }
            [BindProperty]
            public Customer Customer { get; set; }
            [BindProperty]
            public Models.Employee Employee { get; set; }
            public async Task<IActionResult> OnGetAsync(int? id)
            {
                if (HttpContext.Session.GetString("PetSession") == null) return RedirectToPage("/account/singnin");

                Account = JsonSerializer.Deserialize<@Models.Account>(HttpContext.Session.GetString("PetSession"));
                //Customer = dBContext.Customers.FirstOrDefault(c => c.CustomerId == Account.CustomerId);
                //Employee = dBContext.Employees.FirstOrDefault(c => c.EmployeeId == Account.EmployeeId);
                Employee = _context.Employees.FirstOrDefault(c => c.EmployeeId == Account.EmployeeId);
                if (id == null || _context.Products == null)
                {
                    return NotFound();
                }
                var product = await _context.Products.Include(p => p.Category).Include(p => p.Pictures).FirstOrDefaultAsync(c => c.ProductId == id);
                if (product == null)
                {
                    return NotFound();
                }
                else
                {
                    Product = product;
                }
                return Page();
            }
            public async Task<IActionResult> OnPost(string imageLink)
            {
                
                var pro = await _context.Products.Include(x=>x.Pictures).FirstOrDefaultAsync(x => x.ProductId == Product.ProductId);
                if (!string.IsNullOrEmpty(imageLink))
                {
                    int maxLength = 200; // Adjust the maximum length as needed
                    pictureProduct.Picture = imageLink.Length <= maxLength ? imageLink : imageLink.Substring(0, maxLength);
                }
                if (pro != null)
                {
                    pro.ProductName = Product.ProductName;
                    /*pro.Pictures = new List<PictureProduct1> { pictureProduct };*/
                    pro.QuantityPerUnit= Product.QuantityPerUnit;
                    pro.UnitPrice= Product.UnitPrice;
                    //pro.Discontinued= Product.Discontinued;
                    pro.Description = Product.Description;
                    //pro.Status= Product.Status;
                    /*pro.CreateDate = Product.CreateDate;*/
                    pro.UpdateDate= DateTime.Now;
                    await _context.SaveChangesAsync();
                    return RedirectToPage("./Index");
                }
                else
                {
                    return Page();
                }
            
            }
        }
    }
