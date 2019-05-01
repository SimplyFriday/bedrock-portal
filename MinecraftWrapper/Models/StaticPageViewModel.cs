using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Models
{
    public class StaticPageViewModel
    {
        public string Title { get; set; }

        public string Content { get; set; }
        public bool HasContent => !string.IsNullOrEmpty ( Content );

    }
}
