using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MinecraftWrapper.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<NewsItem> NewsItems { get; set; }
    }
}
