using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;

namespace Project_Group5.Pages.Admin.DashBoard
{
    [Authorize(Roles = "1")]
    public class DashboardModel : PageModel
    {
            private readonly Fall24_SE1745_PRN221_Group5Context projectContext;

            public DashboardModel(Fall24_SE1745_PRN221_Group5Context projectContext)
            {
                this.projectContext = projectContext;
            }

        }
    }
