using MinecraftWrapper.Data.Entities;
using System.Collections.Generic;

namespace MinecraftWrapper.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public IEnumerable<NewsItem> NewsItems { get; set; }
    }
}
