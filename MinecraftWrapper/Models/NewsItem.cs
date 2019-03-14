using System;
using System.ComponentModel.DataAnnotations;

namespace MinecraftWrapper.Models
{
    public class NewsItem
    {
        public Guid NewsItemId { get; set; }

        public DateTime DateActive { get; set; }

        public DateTime? DateExpires { get; set; }

        [Required]
        public string HtmlContent { get; set; }

        public string Title { get; set; }
    }
}
